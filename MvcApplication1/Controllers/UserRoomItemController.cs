using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MvcApplication1.Models;

namespace MvcApplication1.Controllers
{
    public class UserRoomItemController : Controller
    {
        private UserRoomContext db = new UserRoomContext();

        // GET: User_Room
        public ActionResult Index()
        {
            return View(db.User_Room.ToList());
        }

        // GET: User_Room/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserRoomItem user_Room = db.User_Room.Find(id);
            if (user_Room == null)
            {
                return HttpNotFound();
            }
            return View(user_Room);
        }

        // GET: User_Room/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: User_Room/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id")] UserRoomItem user_Room)
        {
            if (ModelState.IsValid)
            {
                db.User_Room.Add(user_Room);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user_Room);
        }

        // GET: User_Room/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserRoomItem user_Room = db.User_Room.Find(id);
            if (user_Room == null)
            {
                return HttpNotFound();
            }
            return View(user_Room);
        }

        // POST: User_Room/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id")] UserRoomItem user_Room)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user_Room).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user_Room);
        }

        // GET: User_Room/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserRoomItem user_Room = db.User_Room.Find(id);
            if (user_Room == null)
            {
                return HttpNotFound();
            }
            return View(user_Room);
        }

        // POST: User_Room/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            UserRoomItem user_Room = db.User_Room.Find(id);
            db.User_Room.Remove(user_Room);
            db.SaveChanges();
            return RedirectToAction("Index");
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
