using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using WPChatServer.Models;

namespace WPChatServer.Controllers
{
    public class UserRoomItemController : ApiController
    {
        private UserRoomItemContext db = new UserRoomItemContext();

        // GET: api/UserRoomItem
        public IQueryable<UserRoomItem> GetUserRoomItems()
        {
            return db.UserRoomItems;
        }

        // GET: api/UserRoomItem/5
        [ResponseType(typeof(UserRoomItem))]
        public IHttpActionResult GetUserRoomItem(int id)
        {
            UserRoomItem userRoomItem = db.UserRoomItems.Find(id);
            if (userRoomItem == null)
            {
                return NotFound();
            }

            return Ok(userRoomItem);
        }

        // PUT: api/UserRoomItem/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutUserRoomItem(int id, UserRoomItem userRoomItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != userRoomItem.Id)
            {
                return BadRequest();
            }

            db.Entry(userRoomItem).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserRoomItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/UserRoomItem
        [ResponseType(typeof(UserRoomItem))]
        public IHttpActionResult PostUserRoomItem(UserRoomItem userRoomItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.UserRoomItems.Add(userRoomItem);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = userRoomItem.Id }, userRoomItem);
        }

        // DELETE: api/UserRoomItem/5
        [ResponseType(typeof(UserRoomItem))]
        public IHttpActionResult DeleteUserRoomItem(int id)
        {
            UserRoomItem userRoomItem = db.UserRoomItems.Find(id);
            if (userRoomItem == null)
            {
                return NotFound();
            }

            db.UserRoomItems.Remove(userRoomItem);
            db.SaveChanges();

            return Ok(userRoomItem);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UserRoomItemExists(int id)
        {
            return db.UserRoomItems.Count(e => e.Id == id) > 0;
        }
    }
}