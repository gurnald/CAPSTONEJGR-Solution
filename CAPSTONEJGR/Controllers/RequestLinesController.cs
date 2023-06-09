﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CAPSTONEJGR.Models;
using System.Runtime.InteropServices;

namespace CAPSTONEJGR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestLinesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RequestLinesController(AppDbContext context)
        {
            _context = context;
        }
       

        //Method to Recalculate request total
            [HttpPut("RecalculateRequestTotal/{requestid}")]
            private async Task<ActionResult> RecalculateTotal(int requestId) {
            var request = await _context.Requests.FindAsync(requestId);
                if (request == null) {
                throw new Exception($" Couldn't find Request {requestId}");
            }

                            request.Total = (from p in _context.Products
                                              join rl in _context.RequestLines
                                                    on p.Id equals rl.ProductId
                                                        where rl.RequestId == requestId
                                                select new {
                                                    LineTotal = p.Price * rl.Quantity
                                                }).Sum(x => x.LineTotal);
                        await _context.SaveChangesAsync();
            return Ok();
        }

        // GET: api/RequestLines
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequestLine>>> GetRequestLines()
        {
            if (_context.RequestLines == null) {

                return NotFound();
            }
            return await _context.RequestLines.Include( x => x.Request).Include(x => x.Product).ToListAsync();
        }

        // GET: api/RequestLines/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RequestLine>> GetRequestLine(int id)
        {
            if (_context.RequestLines == null)

            {
                return NotFound();
            }

            var requestLine = await _context.RequestLines.Include(x => x.Request)
                                                        .Include(x => x.Product).SingleOrDefaultAsync(x => x.Id == id);
                        if (requestLine == null) {

                return NotFound();
            }
            
            return requestLine;
        }

        // PUT: api/RequestLines/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRequestLine(int id, RequestLine requestLine)
        {
            if (id != requestLine.Id)
            {
                return BadRequest();
            }

            _context.Entry(requestLine).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                await RecalculateTotal(requestLine.RequestId); 
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RequestLineExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/RequestLines
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<RequestLine>> PostRequestLine(RequestLine requestLine)
        {

            if(requestLine.Quantity < 1) {
                throw new Exception("Quantity must be positive number!");

            }
             
            _context.RequestLines.Add(requestLine);
            await _context.SaveChangesAsync();
            await RecalculateTotal(requestLine.RequestId);

            return CreatedAtAction("GetRequestLine", new { id = requestLine.Id }, requestLine);
        }

        // DELETE: api/RequestLines/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequestLine(int id)
        {
            var requestLine = await _context.RequestLines.FindAsync(id);
            if (requestLine == null)
            {
                return NotFound();
            }

            _context.RequestLines.Remove(requestLine);
            await _context.SaveChangesAsync();
            await RecalculateTotal(requestLine.RequestId);

            return NoContent();
        }

        private bool RequestLineExists(int id)
        {
            return _context.RequestLines.Any(e => e.Id == id);
        }
    }
}
