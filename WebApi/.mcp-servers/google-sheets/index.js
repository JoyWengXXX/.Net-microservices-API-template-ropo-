#!/usr/bin/env node
/**
 * MCP Server: Google Sheets Work Log
 *
 * 認證方式：Google Service Account (JSON credentials file)
 * 環境變數：
 *   GOOGLE_CREDENTIALS_FILE  — Service Account JSON 金鑰檔案路徑
 *   SPREADSHEET_ID           — Google Spreadsheet ID（URL 中的那段 ID）
 *   SHEET_NAME               — 工作日報 Sheet 名稱（預設：WorkLog）
 */

import { Server } from '@modelcontextprotocol/sdk/server/index.js';
import { StdioServerTransport } from '@modelcontextprotocol/sdk/server/stdio.js';
import {
  CallToolRequestSchema,
  ListToolsRequestSchema,
} from '@modelcontextprotocol/sdk/types.js';
import { google } from 'googleapis';
import fs from 'fs';

// ─── 環境變數讀取 ─────────────────────────────────────────────────────────────

const CREDENTIALS_FILE = process.env.GOOGLE_CREDENTIALS_FILE;
const SPREADSHEET_ID   = process.env.SPREADSHEET_ID;
const SHEET_NAME       = process.env.SHEET_NAME ?? 'WorkLog';

if (!CREDENTIALS_FILE || !SPREADSHEET_ID) {
  console.error(
    '[mcp-google-sheets] 缺少必要環境變數：GOOGLE_CREDENTIALS_FILE 或 SPREADSHEET_ID'
  );
  process.exit(1);
}

if (!fs.existsSync(CREDENTIALS_FILE)) {
  console.error(`[mcp-google-sheets] 找不到 credentials 檔案：${CREDENTIALS_FILE}`);
  process.exit(1);
}

// ─── Google Sheets 輔助函式 ────────────────────────────────────────────────────

// 啟動時讀取一次 credentials 並快取，避免每次 API 呼叫都做磁碟 I/O
let _authClient = null;
function buildAuthClient() {
  if (_authClient) return _authClient;
  let credentials;
  try {
    credentials = JSON.parse(fs.readFileSync(CREDENTIALS_FILE, 'utf8'));
  } catch (e) {
    throw new Error(`無法讀取或解析 credentials 檔案：${e.message}`);
  }
  _authClient = new google.auth.GoogleAuth({
    credentials,
    scopes: ['https://www.googleapis.com/auth/spreadsheets'],
  });
  return _authClient;
}

/** 確認 Sheet 存在；不存在則建立並加入標題列 */
async function ensureSheetExists() {
  const auth        = buildAuthClient();
  const sheetsApi   = google.sheets({ version: 'v4', auth });
  const spreadsheet = await sheetsApi.spreadsheets.get({ spreadsheetId: SPREADSHEET_ID });

  const exists = spreadsheet.data.sheets?.some(
    (s) => s.properties?.title === SHEET_NAME
  );

  if (exists) {
    return { created: false, message: `Sheet "${SHEET_NAME}" 已存在` };
  }

  // 建立新分頁
  await sheetsApi.spreadsheets.batchUpdate({
    spreadsheetId: SPREADSHEET_ID,
    requestBody: {
      requests: [{ addSheet: { properties: { title: SHEET_NAME } } }],
    },
  });

  // 寫入標題列
  await sheetsApi.spreadsheets.values.update({
    spreadsheetId: SPREADSHEET_ID,
    range: `${SHEET_NAME}!A1:H1`,
    valueInputOption: 'RAW', // 標題列也用 RAW，避免公式注入
    requestBody: {
      values: [['日期', '星期', 'Commit Hash', 'Type', 'Scope', '工作摘要', '異動檔案數', 'Commit 完整訊息']],
    },
  });

  return { created: true, message: `Sheet "${SHEET_NAME}" 已建立並完成初始化` };
}

/**
 * 附加一筆工作日報記錄
 * @param {object} params
 * @param {string} params.date          - 日期 (YYYY-MM-DD)
 * @param {string} params.weekday       - 星期（中文，例如 週一）
 * @param {string} params.commitHash    - Short commit hash
 * @param {string} [params.type]        - Conventional Commits type
 * @param {string} [params.scope]       - Conventional Commits scope
 * @param {string} params.summary       - 工作摘要（commit subject 的描述部分）
 * @param {number} [params.filesChanged]- 異動檔案數
 * @param {string} [params.fullMessage] - commit 完整訊息
 */
async function appendWorkLogRow(params) {
  const {
    date,
    weekday   = '',
    commitHash,
    type      = '',
    scope     = '',
    summary,
    filesChanged = 0,
    fullMessage  = '',
  } = params;

  const auth     = buildAuthClient();
  const sheetsApi = google.sheets({ version: 'v4', auth });

  const row = [date, weekday, commitHash, type, scope, summary, filesChanged, fullMessage];

  // 使用 RAW 而非 USER_ENTERED，防止儲存格公式注入（OWASP A03 Injection）
  await sheetsApi.spreadsheets.values.append({
    spreadsheetId: SPREADSHEET_ID,
    range: `${SHEET_NAME}!A:H`,
    valueInputOption: 'RAW',
    requestBody: { values: [row] },
  });

  const label = type
    ? `${type}${scope ? `(${scope})` : ''}: ${summary}`
    : summary;

  return {
    success: true,
    message: `✅ 已新增：[${date} ${weekday}] ${commitHash} — ${label}`,
  };
}

/**
 * 讀取最近的工作日報記錄
 * @param {object} params
 * @param {number} [params.limit=10] - 要讀取的行數
 */
const READ_LIMIT_MAX = 500; // 防止一次讀取過多資料造成記憶體問題

async function readWorkLog({ limit = 10 } = {}) {
  const safeLimit = Math.min(Math.max(1, Number(limit) || 10), READ_LIMIT_MAX);

  const auth      = buildAuthClient();
  const sheetsApi = google.sheets({ version: 'v4', auth });

  const response = await sheetsApi.spreadsheets.values.get({
    spreadsheetId: SPREADSHEET_ID,
    range: `${SHEET_NAME}!A:H`,
  });

  const rows     = response.data.values ?? [];
  const dataRows = rows.slice(1).slice(-safeLimit); // 跳過標題列，取最後 N 筆

  return {
    count: dataRows.length,
    entries: dataRows.map((r) => ({
      date:         r[0] ?? '',
      weekday:      r[1] ?? '',
      commitHash:   r[2] ?? '',
      type:         r[3] ?? '',
      scope:        r[4] ?? '',
      summary:      r[5] ?? '',
      filesChanged: r[6] ?? '0',
      fullMessage:  r[7] ?? '',
    })),
  };
}

// ─── MCP Server 定義 ───────────────────────────────────────────────────────────

const server = new Server(
  { name: 'google-sheets', version: '1.0.0' },
  { capabilities: { tools: {} } }
);

server.setRequestHandler(ListToolsRequestSchema, async () => ({
  tools: [
    {
      name: 'append_work_log',
      description: '將一筆 git commit 記錄新增到 Google Sheet 工作日報',
      inputSchema: {
        type: 'object',
        properties: {
          date:         { type: 'string',  description: '日期（YYYY-MM-DD），例如 2026-03-08' },
          weekday:      { type: 'string',  description: '星期（中文），例如 週六' },
          commitHash:   { type: 'string',  description: 'Short commit hash，例如 a1b2c3d' },
          type:         { type: 'string',  description: 'Conventional Commits type（feat/fix/refactor/...）' },
          scope:        { type: 'string',  description: 'Conventional Commits scope' },
          summary:      { type: 'string',  description: '工作摘要（commit subject 的描述部分）' },
          filesChanged: { type: 'number',  description: '異動檔案數' },
          fullMessage:  { type: 'string',  description: 'Commit 完整訊息（type(scope): subject\\n\\nbody）' },
        },
        required: ['date', 'commitHash', 'summary'],
      },
    },
    {
      name: 'read_work_log',
      description: '讀取 Google Sheet 工作日報的最近記錄',
      inputSchema: {
        type: 'object',
        properties: {
          limit: { type: 'number', description: '要讀取的最近筆數（預設 10）', default: 10 },
        },
      },
    },
    {
      name: 'init_work_log_sheet',
      description: '初始化工作日報 Sheet（若不存在則自動建立並加入標題列）',
      inputSchema: {
        type: 'object',
        properties: {},
      },
    },
  ],
}));

server.setRequestHandler(CallToolRequestSchema, async (request) => {
  const { name, arguments: args } = request.params;

  try {
    let result;
    switch (name) {
      case 'append_work_log':
        result = await appendWorkLogRow(args);
        break;
      case 'read_work_log':
        result = await readWorkLog(args);
        break;
      case 'init_work_log_sheet':
        result = await ensureSheetExists();
        break;
      default:
        throw new Error(`Unknown tool: ${name}`);
    }

    return {
      content: [{ type: 'text', text: JSON.stringify(result, null, 2) }],
    };
  } catch (error) {
    return {
      content: [{ type: 'text', text: `Error: ${error.message}` }],
      isError: true,
    };
  }
});

// ─── 啟動 ──────────────────────────────────────────────────────────────────────

const transport = new StdioServerTransport();
await server.connect(transport);
