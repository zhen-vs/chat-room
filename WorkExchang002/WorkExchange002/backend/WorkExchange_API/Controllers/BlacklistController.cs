using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkExchange_API.Models;
using WorkExchange_API.Services;


namespace WorkExchange_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/blacklist")]
    public class BlacklistController : ControllerBase
    {
        private readonly BlacklistService _blacklistService;
        public BlacklistController(BlacklistService blacklistService)
        {
            _blacklistService = blacklistService;
        }

        ///取得目前操作的使用者 ID(身分驗證)
        /// 優先使用登入 Claim，沒有就改用 X-Debug-UserId

        private int GetMyId()
        {
            // 嘗試從 Token (Claim) 裡面抓取 "UserId" 或標準的 "NameIdentifier" 欄位 -登入修好從這拿
            var raw = User.FindFirstValue("UserId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 如果抓得到 ID，就把它轉成整數回傳 int.TryParse 1.不噴錯： 即使轉換失敗，它也只是回傳 false /2.效能好
            if (int.TryParse(raw, out var id)) return id;


            //都拿不到使用者 ID丟(未授權例外)
            throw new UnauthorizedAccessException("找不到使用者");
        }

        //查詢封鎖狀態 // 路由參數，例如 GET /api/blacklist/
        [HttpGet("{blockedId:int}")]
        public async Task<IActionResult> GetStatus(int blockedId)
        {
            var blockerId = GetMyId();

            if (blockerId == blockedId)
            {
                return BadRequest(new { message = "不可封鎖自己" });
            }

            //查找封鎖紀錄
            var exists = await _blacklistService.IsBlockedAsync(blockerId, blockedId);



            //回傳結果
            return Ok(new { blockerId, blockedId, isBlocked = exists });
        }

        //執行封鎖 
        [HttpPost("{blockedId:int}")]
        public async Task<IActionResult> Block(int blockedId)
        {
            var blockerId = GetMyId();

            if (blockerId == blockedId)
            {
                return BadRequest(new { message = "不可封鎖自己" });
            }


            await _blacklistService.BlockAsync(blockerId, blockedId);
            return Ok(new { blockerId, blockedId, isBlocked = true });


        }
        // 解除封鎖 
        [HttpDelete("{blockedId:int}")]

        public async Task<IActionResult> Unblock(int blockedId)
        {
            var blockerId = GetMyId();

            await _blacklistService.UnblockAsync(blockerId, blockedId);

            return Ok(new { blockerId, blockedId, isBlocked = false });

        }
    }
}

