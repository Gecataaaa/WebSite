using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WebProjectNbu12.Models;

namespace WebProjectNbu12.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Action method for rendering the main page
        public ActionResult Index()
        {
            // Load the last comments before rendering the view
            LoadLastComments();
            return View();
        }

        // Action method for handling comment submission (HTTP POST)
        [HttpPost]
        public ActionResult SubmitComment([FromBody] Comment comment)
        {
            if (comment != null)
            {
                // Save the submitted comment to the file
                SaveCommentToFile(comment);
            }

            // Redirect back to the main page after submitting the comment
            return RedirectToAction("Index");
        }

        // Action method for loading the last comments (HTTP GET)
        [HttpGet]
        public JsonResult LoadLastComments()
        {
            // Load all comments from the file
            List<Comment> comments = LoadCommentsFromFile();

            // Select the last three comments
            int startIndex = Math.Max(0, comments.Count - 3);
            var lastComments = comments.Skip(startIndex).Take(Math.Min(3, comments.Count - startIndex)).ToList();

            // Set the last comments in the ViewBag for use in the view
            ViewBag.LastComments = lastComments;

            // Return the last three comments as JSON
            return Json(lastComments);
        }

        // Method for saving a comment to the file
        private void SaveCommentToFile(Comment comment)
        {
            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "comments.txt");

                // Read existing comments from the file
                List<Comment> existingComments = LoadCommentsFromFile();

                // Add the new comment to the existing comments
                existingComments.Add(comment);

                // Write all comments back to the file (overwrite the file)
                using (StreamWriter writer = new StreamWriter(filePath, false))
                {
                    foreach (var existingComment in existingComments)
                    {
                        writer.WriteLine($"{existingComment.Name}: {existingComment.Text}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log an error if there is an issue saving the comment
                _logger.LogError($"Error saving comment to file: {ex.Message}");
            }
        }

        // Method for loading comments from the file
        private List<Comment> LoadCommentsFromFile()
        {
            List<Comment> comments = new List<Comment>();
            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "comments.txt");

                // Check if the file exists
                if (System.IO.File.Exists(filePath))
                {
                    // Read all lines from the file
                    string[] lines = System.IO.File.ReadAllLines(filePath);

                    // Parse each line into a Comment object and add it to the list
                    foreach (string line in lines)
                    {
                        var parts = line.Split(':');
                        if (parts.Length == 2)
                        {
                            var comment = new Comment
                            {
                                // Use null-coalescing operators to handle potential null values
                                Name = parts[0]?.Trim() ?? "DefaultName",
                                Text = parts[1]?.Trim() ?? "DefaultText"
                            };

                            comments.Add(comment);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log an error if there is an issue loading comments from the file
                _logger.LogError($"Error loading comments from file: {ex.Message}");
            }

            return comments;
        }
    }
}
