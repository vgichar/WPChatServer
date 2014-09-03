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
    public class FriendRequestsController : ApiController
    {
        private FriendRequestContext db = new FriendRequestContext();

        // GET: api/FriendRequests
        public IQueryable<FriendRequest> GetFriendRequests()
        {
            return db.FriendRequests;
        }

        // GET: api/FriendRequests/5
        [ResponseType(typeof(FriendRequest))]
        public IHttpActionResult GetFriendRequest(int id)
        {
            FriendRequest friendRequest = db.FriendRequests.Find(id);
            if (friendRequest == null)
            {
                return NotFound();
            }

            return Ok(friendRequest);
        }

        // PUT: api/FriendRequests/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutFriendRequest(int id, FriendRequest friendRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != friendRequest.Id)
            {
                return BadRequest();
            }

            db.Entry(friendRequest).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FriendRequestExists(id))
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

        // POST: api/FriendRequests
        [ResponseType(typeof(FriendRequest))]
        public IHttpActionResult PostFriendRequest(FriendRequest friendRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.FriendRequests.Add(friendRequest);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = friendRequest.Id }, friendRequest);
        }

        // DELETE: api/FriendRequests/5
        [ResponseType(typeof(FriendRequest))]
        public IHttpActionResult DeleteFriendRequest(int id)
        {
            FriendRequest friendRequest = db.FriendRequests.Find(id);
            if (friendRequest == null)
            {
                return NotFound();
            }

            db.FriendRequests.Remove(friendRequest);
            db.SaveChanges();

            return Ok(friendRequest);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool FriendRequestExists(int id)
        {
            return db.FriendRequests.Count(e => e.Id == id) > 0;
        }
    }
}