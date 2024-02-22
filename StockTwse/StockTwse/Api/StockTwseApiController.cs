using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockTwse.ViewModels;
using System;
using System.Net.Http;
using System.Text.Json;

namespace StockTwse.Api {
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class StockTwseApiController : ControllerBase {
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;

		public StockTwseApiController(IHttpClientFactory httpClientFactory, IConfiguration configuration) {
			// IHttpClientFactory 取代靜態 httpClient, 避免長期重複使用同一個 client 導致無法更新 DNS 訊息, 或是開啟過多 client 導致占用大量 port 號
			_httpClientFactory = httpClientFactory;
			// 讀取 appsettings.json
			_configuration = configuration;
		}

		[HttpGet("{userInput}")]
		public async Task<ActionResult> GetStockInfo(string userInput) {
			// 整股交易行情
			// tse開頭為上市股票。
			// otc開頭為上櫃股票。
			// 如果是興櫃股票則無法取得。
			// 上市公司股票代號查詢
			string tseUrl = $"https://mis.twse.com.tw/stock/api/getStockInfo.jsp?ex_ch=tse_{userInput}.tw";
			// 上櫃公司股票代號查詢
			string otcUrl = $"https://mis.twse.com.tw/stock/api/getStockInfo.jsp?ex_ch=otc_{userInput}.tw";

			// 盤中零股行情
			string tseUrlOdd = $"https://mis.twse.com.tw/stock/api/getOddInfo.jsp?ex_ch=tse_{userInput}.tw";
			string otcUrlOdd = $"https://mis.twse.com.tw/stock/api/getOddInfo.jsp?ex_ch=otc_{userInput}.tw";

			// 請求 data
			HttpClient client = _httpClientFactory.CreateClient();

			// 發起請求
			// 先用 tseUrl 請求, 如果返回的 msgArray 是空的, 就用 otcUrl 再請求一次
			HttpResponseMessage responseTse = await client.GetAsync(tseUrl);

			// 如果回傳的 http code 不是200, 拋出例外
			//response.EnsureSuccessStatusCode();

			if (!responseTse.IsSuccessStatusCode) {
				return BadRequest(await responseTse.Content.ReadAsStringAsync());
			}

			// 轉成字串
			string responseBodyTse = await responseTse.Content.ReadAsStringAsync();

			// 先用 tseUrl 請求, 判斷返回的 msgArray 是不是空的
			JsonDocument jd = JsonDocument.Parse(responseBodyTse);

			JsonElement root = jd.RootElement;

			JsonElement msgArray = root.GetProperty("msgArray");

			if (msgArray.ValueKind == JsonValueKind.Array && msgArray.EnumerateArray().Any()) {
				return Ok(responseBodyTse);
			}

			// 發起請求
			// 先用 tseUrl 請求, 如果返回的 msgArray 是空的, 就用 otcUrl 再請求一次
			HttpResponseMessage responseOtc = await client.GetAsync(otcUrl);

			if (!responseOtc.IsSuccessStatusCode) {
				return BadRequest(await responseOtc.Content.ReadAsStringAsync());
			}

			// 轉成字串
			string responseBodyOtc = await responseOtc.Content.ReadAsStringAsync();

			return Ok(responseBodyOtc);

			// 回傳的JSON欄位說明：

			// 欄位名稱	描述
			// z	當前盤中成交價
			// tv	當前盤中盤成交量
			// v	累積成交量
			// b	揭示買價(從高到低，以_分隔資料)
			// g	揭示買量(配合b，以_分隔資料)
			// a	揭示賣價(從低到高，以_分隔資料)
			// f	揭示賣量(配合a，以_分隔資料)
			// o	開盤價格
			// h	最高價格
			// l	最低價格
			// y	昨日收盤價格
			// u	漲停價
			// w	跌停價
			// tlong	資料更新時間（單位：毫秒）
			// d	最近交易日期（YYYYMMDD）
			// t	最近成交時刻（HH:MI:SS）
			// c	股票代號
			// n	公司簡稱
			// nf	公司全名
		}
	}
}
