﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using BookingBio.Models;

namespace BookingBio.Controllers
{
    public class MovieShowingsController : ApiController
    {
        private BookingDBEntities db = new BookingDBEntities();

        // GET: api/MovieShowings
        public IQueryable<MovieShowings> GetMovieShowings()
        {
            return db.MovieShowings;
        }

        // GET: api/MovieShowings/5
        [ResponseType(typeof(MovieShowings))]
        public IHttpActionResult GetMovieShowings(int id)
        {
            MovieShowings movieShowings = db.MovieShowings.Find(id);
            if (movieShowings == null)
            {
                return NotFound();
            }

            return Ok(movieShowings);
        }

        // PUT: api/MovieShowings/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutMovieShowings(int id, MovieShowings movieShowings)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != movieShowings.movieShowingsId)
            {
                return BadRequest();
            }

            db.Entry(movieShowings).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieShowingsExists(id))
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

        // POST: api/MovieShowings
        [ResponseType(typeof(MovieShowings))]
        public IHttpActionResult PostMovieShowings(MovieShowings movieShowings)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.MovieShowings.Add(movieShowings);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = movieShowings.movieShowingsId }, movieShowings);
        }

        // DELETE: api/MovieShowings/5
        [ResponseType(typeof(MovieShowings))]
        public IHttpActionResult DeleteMovieShowings(int id)
        {
            MovieShowings movieShowings = db.MovieShowings.Find(id);
            if (movieShowings == null)
            {
                return NotFound();
            }

            db.MovieShowings.Remove(movieShowings);
            db.SaveChanges();

            return Ok(movieShowings);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool MovieShowingsExists(int id)
        {
            return db.MovieShowings.Count(e => e.movieShowingsId == id) > 0;
        }
    }
}