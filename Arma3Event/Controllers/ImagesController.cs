using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Arma3Event.Controllers
{
    [Authorize(Policy = "Admin")]
    public class ImagesController : Controller
    {
        private static readonly FileExtensionContentTypeProvider typeProvider = new FileExtensionContentTypeProvider();

        public ImagesController()
        {

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadJson(IFormFile image)
        {
            var ext = typeProvider.Mappings.FirstOrDefault(p => string.Equals(p.Value, image.ContentType, StringComparison.OrdinalIgnoreCase)).Key ?? ".jpeg";
            var target = Path.Combine("img", "uploaded", Guid.NewGuid() + ext);
            using (var stream = System.IO.File.Create(Path.Combine("wwwroot", target)))
            {
                await image.CopyToAsync(stream);
            }
            return Json("/" + target.Replace("\\","/"));
        }

        [HttpGet]
        public IActionResult ListJson()
        {
            var files = Directory.GetFiles(Path.Combine("wwwroot", "img", "maps"), "*", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(Path.Combine("wwwroot", "img", "uploaded"), "*", SearchOption.AllDirectories));

            var normalized = files.Select(f => f.Replace("\\", "/").Replace("wwwroot/", "/")).ToList();

            return Json(normalized);
        }
    }
}