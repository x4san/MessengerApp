using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessengerApp.Data;

namespace MessengerApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // Страница консоли
        [HttpGet("/Admin/SqlConsole")]
        public IActionResult SqlConsole()
        {
            return View("/Views/Admin/SqlConsole.cshtml");
        }

        // Выполнение SQL (SELECT/PRAGMA/WITH -> resultset; остальное -> nonquery)
        [ValidateAntiForgeryToken]
        [HttpPost("/Admin/Execute")]
        public async Task<IActionResult> Execute([FromForm] string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return BadRequest("SQL пустой.");

            await using var conn = _context.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandTimeout = 30;

            try
            {
                var firstToken = sql.TrimStart()
                    .Split(new[] { ' ', '\t', '\r', '\n' }, 2)[0]
                    .ToUpperInvariant();

                if (firstToken == "SELECT" || firstToken == "PRAGMA" || firstToken == "WITH")
                {
                    await using var reader = await cmd.ExecuteReaderAsync();
                    var table = new DataTable();
                    table.Load(reader);

                    var columns = table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
                    var rows = table.Rows.Cast<DataRow>()
                        .Select(r => r.ItemArray.Select(x => x?.ToString()).ToArray())
                        .ToArray();

                    return Json(new
                    {
                        ok = true,
                        type = "resultset",
                        columns,
                        rows,
                        rowCount = rows.Length
                    });
                }
                else
                {
                    var affected = await cmd.ExecuteNonQueryAsync();
                    return Json(new
                    {
                        ok = true,
                        type = "nonquery",
                        affected
                    });
                }
            }
            catch (System.Exception ex)
            {
                Response.StatusCode = 400;
                return Json(new
                {
                    ok = false,
                    error = ex.Message
                });
            }
        }
    }
}
