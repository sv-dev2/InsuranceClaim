using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Insurance.Domain;
using InsuranceClaim.Models;
using AutoMapper;



namespace InsuranceClaim.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product 


        public ActionResult Index()
        {
            //InsuranceContext.Products.Insert();
            InsuranceClaim.Models.ProductModel obj = new InsuranceClaim.Models.ProductModel();
            List<Insurance.Domain.Product> objList = new List<Insurance.Domain.Product>();
            objList = InsuranceContext.Products.All().ToList();
                
            return View(obj);
        }
        [HttpPost]
        public ActionResult ProductSave(ProductModel model)
        {
            var dbModel = Mapper.Map<ProductModel, Product>(model);
            InsuranceContext.Products.Insert(dbModel);
            return RedirectToAction("ProductList");
        }
        [Authorize(Roles = "Administrator")]
        public ActionResult ProductList()
        {
            var db = InsuranceContext.Products.All(where:"Active ='True' or Active is null").ToList();


            return View(db);
        }
        public ActionResult ProductEdit(int Id)
        {
            var record = InsuranceContext.Products.All(where: $"Id ={Id}").FirstOrDefault();

            var model = Mapper.Map<Product, ProductModel>(record);
            return View(model);
        }
        [HttpPost]
        public ActionResult ProductEdit(ProductModel model)
        {
            if (ModelState.IsValid)
            {
                //var db = InsuranceContext.Products.Single(where: $"Id = {model.Id}");


                var data = Mapper.Map<ProductModel, Product>(model);
                //db.ProductName = model.ProductName;
                //db.ProductCode = model.ProductCode;
                InsuranceContext.Products.Update(data);

            }

            return RedirectToAction("ProductList");
        }
        public ActionResult DeleteProduct(int Id )
        {
            string query = $"update Product set Active=0 where Id={Id}";
            InsuranceContext.Products.Execute(query);

            return RedirectToAction("ProductList");
        }
    }
}
