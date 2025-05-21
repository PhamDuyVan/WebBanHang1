using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.Models;
namespace WebBanHang.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _hosting;
        public ProductController(ApplicationDbContext db, IWebHostEnvironment hosting)
        {
            _db = db;
            _hosting = hosting;
        }
        public IActionResult Index(int page = 1)
        {
            int pageSize = 4;
            var currentPage = page;
            var dsSanPham = _db.Products.Include(x => x.Category).ToList();
            //Truyen du lieu cho View
            ViewBag.PageSum = Math.Ceiling((double)dsSanPham.Count / pageSize);
            ViewBag.CurrentPage = currentPage;

            return View(dsSanPham.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList());
        }
        public IActionResult Delete(int id)
        {
            var product = _db.Products.Find(id);       
            return View(product);
        }
        public IActionResult DeleteConfirm(int id)
        {
            var product = _db.Products.Find(id);
            if (!String.IsNullOrEmpty(product.ImageUrl))
            {
                var oldFilePath = Path.Combine(_hosting.WebRootPath, product.ImageUrl);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }
            _db.Products.Remove(product);
            _db.SaveChanges();
            return RedirectToAction("Index");  
        }
        public IActionResult Add()
        {
            //truyền danh sách thể loại cho View để sinh ra điều khiển DropDownList
            ViewBag.DSTL = _db.Categories.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name });
            return View();
        }
        [HttpPost]
        public IActionResult Add(Product p ,IFormFile ImageUrl)
        {
            if (ModelState.IsValid) //kiem tra hop le
            {
                if (ImageUrl != null)
                {
                    //xu ly upload và lưu ảnh đại diện
                    p.ImageUrl = SaveImage(ImageUrl);
                }
                //thêm product vào table Product
                _db.Products.Add(p);
                _db.SaveChanges();
                TempData["success"] = "Product inserted success";
                return RedirectToAction("Index");
            }
            ViewBag.DSTL = _db.Categories.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name });
            return View();
        }
        private string SaveImage(IFormFile image)
        {
            //đặt lại tên file cần lưu
            var filename = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            //lay duong dan luu tru wwwroot tren server
            var path = Path.Combine(_hosting.WebRootPath, @"images/products");
            var saveFile = Path.Combine(path, filename);
            using (var filestream = new FileStream(saveFile, FileMode.Create))
            {
                image.CopyTo(filestream);
            }
            return @"images/products/" + filename;
        }
        public IActionResult Update(int id)
        {
            var product = _db.Products.Find(id);
            //truyền danh sách thể loại cho View để sinh ra điều khiển DropDownList
            ViewBag.DSTL = _db.Categories.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name });
            return View(product);
        }
        [HttpPost]
        public IActionResult Update(Product p, IFormFile ImageUrl)
        {
            var OldP = _db.Products.Find(p.Id);
                if (ImageUrl != null)
                {
                    //xu ly upload và lưu ảnh đại diện
                    p.ImageUrl = SaveImage(ImageUrl);
                }
            else
            {
                p.ImageUrl = OldP.ImageUrl;
            }
            //thêm product vào table Product
            //cập nhật product vào table Product
            OldP.Name = p.Name;
            OldP.Description = p.Description;
            OldP.Price = p.Price;
            OldP.CategoryId = p.CategoryId;
            OldP.ImageUrl = p.ImageUrl;
            _db.SaveChanges();
                TempData["success"] = "Product inserted success";
                return RedirectToAction("Index");
        }

    }
}
