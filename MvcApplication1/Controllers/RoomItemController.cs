using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using MvcApplication1.Models;

namespace MvcApplication1.Controllers
{
    public class RoomItemController : ApiController
    {
        private RoomItemContext db = new RoomItemContext();

        // GET api/RoomItemController
        public IEnumerable<RoomItem> GetRoomItems()
        {
            return db.RoomItems.AsEnumerable();
        }

        // GET api/RoomItemController/5
        public RoomItem GetRoomItem(string id)
        {
            RoomItem roomitem = db.RoomItems.Find(id);
            if (roomitem == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return roomitem;
        }

        // PUT api/RoomItemController/5
        public HttpResponseMessage PutRoomItem(string id, RoomItem roomitem)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != roomitem.Name)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Entry(roomitem).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST api/RoomItemController
        public HttpResponseMessage PostRoomItem(RoomItem roomitem)
        {
            if (ModelState.IsValid)
            {
                db.RoomItems.Add(roomitem);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, roomitem);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = roomitem.Name }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/RoomItemController/5
        public HttpResponseMessage DeleteRoomItem(string id)
        {
            RoomItem roomitem = db.RoomItems.Find(id);
            if (roomitem == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.RoomItems.Remove(roomitem);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, roomitem);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}