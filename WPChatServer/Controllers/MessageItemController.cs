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
using WPChatServer.Models;

namespace WPChatServer.Controllers
{
    public class MessageItemController : ApiController
    {
        private MessageItemContext db = new MessageItemContext();

        // GET api/MessageItem
        public IEnumerable<MessageItem> GetMessageItems()
        {
            return db.MessageItems.AsEnumerable();
        }

        // GET api/MessageItem/5
        public MessageItem GetMessageItem(int id)
        {
            MessageItem messageitem = db.MessageItems.Find(id);
            if (messageitem == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return messageitem;
        }

        // PUT api/MessageItem/5
        public HttpResponseMessage PutMessageItem(int id, MessageItem messageitem)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != messageitem.Id)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Entry(messageitem).State = EntityState.Modified;

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

        // POST api/MessageItem
        public HttpResponseMessage PostMessageItem(MessageItem messageitem)
        {
            if (ModelState.IsValid)
            {
                db.MessageItems.Add(messageitem);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, messageitem);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = messageitem.Id }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/MessageItem/5
        public HttpResponseMessage DeleteMessageItem(int id)
        {
            MessageItem messageitem = db.MessageItems.Find(id);
            if (messageitem == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.MessageItems.Remove(messageitem);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, messageitem);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}