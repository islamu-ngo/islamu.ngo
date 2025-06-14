using iLoveIbadah.Website.Contracts;
using iLoveIbadah.Website.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace iLoveIbadah.Website.Controllers
{
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;
        
        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        // GET: CommentController/Create
        //public IActionResult Create()
        //{
        //    return View();
        //}

        // POST: CommentController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCommentVM comment)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _commentService.Create(comment);
                    if (response.Success)
                    {
                        TempData["Success"] = "Comment added successfully!";
                        return RedirectToAction("Details", "Blog", new { id = comment.BlogId });
                    }

                    foreach (var error in response.ValidationErrors)
                    {
                        ModelState.AddModelError("", error.ToString());
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            //return RedirectToAction(nameof(Index));
            //return RedirectToAction("Details", "Blog", new { id = comment.BlogId });
            TempData["Error"] = "Failed to add comment.";
            return RedirectToAction("Details", "Blog", new { id = comment.BlogId });
        }

        // GET: CommentController/Edit/5
        //public ActionResult Update(int id)
        //{
        //    return View();
        //}

        // POST: CommentController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateCommentVM comment, int blogId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _commentService.Update(comment);
                    if (response.Success)
                    {
                        TempData["Success"] = "Comment updated successfully!";
                        return RedirectToAction("Details", "Blog", new { id = blogId });
                    }

                    //false error that comes like nullreference or something but it actualy updates!
                    //foreach (var error in response.ValidationErrors)
                    //{
                    //    ModelState.AddModelError("", error.ToString());
                    //}
                    //ModelState.AddModelError("", response.ValidationErrors);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            TempData["Error"] = "Failed to update comment.";
            return RedirectToAction("Details", "Blog", new { id = blogId });
        }
    }
}
