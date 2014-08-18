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
using WPChatServer.Models;

namespace WPChatServer.Controllers
{
    public class OwnerUserItemController : ApiController
    {
        private OwnerUserItemContext db = new OwnerUserItemContext();

        // GET api/OwnerUserItem
        public IEnumerable<OwnerUserItem> GetOwnerUserItems()
        {
            return db.OwnerUserItems.AsEnumerable();
        }

        // GET api/OwnerUserItem/5
        public OwnerUserItem GetOwnerUserItem(string id)
        {
            OwnerUserItem owneruseritem = db.OwnerUserItems.Find(id);
            if (owneruseritem == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return owneruseritem;
        }

        // PUT api/OwnerUserItem/5
        public HttpResponseMessage PutOwnerUserItem(string id, OwnerUserItem owneruseritem)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != owneruseritem.Username)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Entry(owneruseritem).State = EntityState.Modified;

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

        // POST api/OwnerUserItem
        public HttpResponseMessage PostOwnerUserItem(OwnerUserItem owneruseritem)
        {
            if (ModelState.IsValid)
            {
                db.OwnerUserItems.Add(owneruseritem);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, owneruseritem);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = owneruseritem.Username }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/OwnerUserItem/5
        public HttpResponseMessage DeleteOwnerUserItem(string id)
        {
            OwnerUserItem owneruseritem = db.OwnerUserItems.Find(id);
            if (owneruseritem == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.OwnerUserItems.Remove(owneruseritem);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, owneruseritem);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}