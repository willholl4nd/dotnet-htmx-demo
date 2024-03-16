using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Htmx;

using dotnet_html_sortable_table.Models;
using dotnet_html_sortable_table.Data;

namespace dotnet_html_sortable_table.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly SqliteContext _context;

    public HomeController(ILogger<HomeController> logger, SqliteContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpGet("Scroll/{id?}")]
    public IActionResult Scroll([FromQuery] int? size, [FromQuery] int? offset = 0, int? id = 1) {
        size ??= 100;
        DemoObject d = _context.TableContainer.First(m => m.Id == id);
        var table = 
            (from row in _context.Entries 
                where row.DemoObjectId == d.Id && row.Id >= size * offset 
                select row)
            .OrderBy(m => m.Id)
            .Take(size.Value)
            .ToList();

        d.Table = table;

        if (HttpContext.Request.IsHtmx())
        {
            offset++;
            ViewData["offset"] = offset;
            ViewData["size"] = size;
            Response.Headers.Add("Vary", "HX-Request");
            return PartialView("_ScrollTable", table);
        } else {
            ViewData["offset"] = 1;
            ViewData["size"] = size;
            return View("InfiniteScroll", d);
        }
    }

    [HttpGet("OffsetInfo/{offset}")]
    public IActionResult OffsetInfo(int offset){
        Response.Headers.Add("Vary", "HX-Request");
        return PartialView("OffsetInfo", offset);
    }

    [HttpGet("Demo/{sortIdx?}")]
    public IActionResult Demo(int? sortIdx, [FromQuery] int? size, [FromQuery] int id = 1) {
        size ??= 100;
        ViewData["Size"] = size;
        DemoObject d = _context.TableContainer.First(m => m.Id == id);
        List<DemoTable> table = 
            (from row in _context.Entries where row.DemoObjectId == d.Id select row).Take(size.Value).ToList();
        d.Table = table;

        if (HttpContext.Request.IsHtmx())
        {
            sortIdx ??= 1;
            ChangeSort(d, sortIdx.Value);

            Response.Headers.Add("Vary", "HX-Request");
            return PartialView("_TableData", d.Table);
        } else {
            return View("Table", d);
        }
    }

    private void ChangeSort(DemoObject data, int sortIdx) {
        foreach (var key in TempData.Keys)
            Console.WriteLine($"Key in TempData: {key}");
        Func<bool, string> sortType = (bool sort) => sort ? "Asc" : "Desc";
        switch (sortIdx) {
            case 1: {
                        bool temp = !data.IdSort;
                        Console.WriteLine($"Sort Id: {sortType(temp)}");
                        data.Table = temp ? data.Table.OrderBy(m => m.Id).ToList() : data.Table.OrderByDescending(m => m.Id).ToList();
                        data.IdSort = temp;
                        break;
                    }
            case 2: {
                        bool temp = !data.RandIntSort;
                        Console.WriteLine($"Sort RandInt: {sortType(temp)}");
                        data.Table = temp ? data.Table.OrderBy(m => m.RandInt).ToList() : data.Table.OrderByDescending(m => m.RandInt).ToList();
                        data.RandIntSort = temp;
                        break;
                    }
            case 3: {
                        bool temp = !data.NameSort;
                        Console.WriteLine($"Sort Name: {sortType(temp)}");
                        data.Table = temp ? data.Table.OrderBy(m => m.Name).ToList() : data.Table.OrderByDescending(m => m.Name).ToList();
                        data.NameSort = temp;
                        break;
                    }

            default: {
                         bool temp = !data.IdSort;
                        Console.WriteLine($"Sort Id: {sortType(temp)}");
                         data.Table = temp ? data.Table.OrderBy(m => m.Id).ToList() : data.Table.OrderByDescending(m => m.Id).ToList();
                         data.IdSort = temp;
                         break;
                     }
        }
        _context.Update(data);
        _context.SaveChanges();
    }
}
