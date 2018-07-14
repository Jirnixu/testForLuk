using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using testforLukoil.Models;

namespace testforLukoil.Controllers
{
    public class usersController : Controller
    {
        private testDBEntities db = new testDBEntities();

        // GET: users
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetUsers()
        {
            List<usersDTO> usersList = db.users.Where(u => u.deleted != 1).Select(u => new usersDTO
            {
                id = u.id,
                email = u.email,
                firstname = u.firstname,
                lastname = u.lastname,

            }).ToList();

            return Json(usersList, JsonRequestBehavior.AllowGet);
        }

        // GET: users/Details/5
        public JsonResult GetUserDetails(int id)
        {
            bool checkUser = db.users.Any(u => u.id == id);
            if (checkUser)
            {
                List<users> UpsModelList = db.users.Where(u => u.id == id).ToList();
                return Json(UpsModelList, JsonRequestBehavior.AllowGet);
            }
            else {

                error Error = new error
                {
                    errorCode = 1,
                    message = "Внутренняя ошибка сервера"
                };

                string jsonError = JsonConvert.SerializeObject(Error, Formatting.Indented);
                return Json(jsonError, JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    users users = db.users.Find(id);
        //    if (users == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(users);
        //}

        // GET: users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "firstname,lastname,email")] users users)
        {
            if (ModelState.IsValid)
            {
                int tmpUser = int.Parse(db.users.OrderByDescending(p => p.id) //
                            .Select(r => r.id)                                // Костылик для автоинкремента id :)
                            .First().ToString());                             //
                
                users.id = tmpUser + 1;
                users.deleted = 0;

                users record = db.users.FirstOrDefault(u => u.email.StartsWith(users.email) && u.email.EndsWith(users.email) && u.deleted==0); // А этот костылик создан для сравнения text с varchar ибо ругается студия, что их сравнивать нельзя
                if (record == null)
                {
                    db.users.Add(users);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                else {
                    ViewBag.error = "Пользователь с таким мылом уже существует";
                    return View();
                }
            }

            return View(users);
        }


        
        public JsonResult DeleteUser(int id)
        {
            var result = false;
            try
            {
                users User = db.users.Find(id);
                User.deleted = 1;
                db.SaveChanges();
                result = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        

        

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
