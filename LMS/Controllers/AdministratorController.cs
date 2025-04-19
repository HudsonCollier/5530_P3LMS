// Implemented by Hudson Collier and Ian Kerr
// Last Edited: April 18, 2025

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly LMSContext db;

        public AdministratorController(LMSContext _db)
        {
            db = _db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
        {
            var query = from d in db.Departments
                        where d.Subject == subject
                        select d;
            if (query.Any())
            {
                return Json(new { success = false });
            }

            try
            {
                Department dNew = new Department();
                dNew.Name = name;
                dNew.Subject = subject;
                db.Departments.Add(dNew);
                db.SaveChanges();
                return Json(new { success = true });
            } catch (Exception)
            {
                return Json(new { success = false });
            }
        }


        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subjCode">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            var query = from d in db.Departments
                        join c in db.Courses on d.Subject equals c.Subject
                        where d.Subject == subject
                        select new
                        {
                            number = c.Num,
                            name = c.Name,
                        };
            return Json(query.ToList());
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {
            var query = from d in db.Departments
                        join p in db.Professors on d.Subject equals p.Department
                        where d.Subject == subject
                        select new
                        {
                            lname = p.LastName,
                            fname = p.FirstName,
                            uid = p.UId,
                        };

            return Json(query.ToArray());
        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {
            var query = from c in db.Courses
                        where c.Subject == subject && c.Num == number
                        select c;
            if (query.Any())
            {
                return Json(new { success = false });
            }

            try
            {
                Course course = new Course();
                course.Subject = subject;
                course.Num = (uint)number;
                course.Name = name;
                db.Add(course);
                db.SaveChanges();
                return Json(new { success = true });
            } catch(Exception)
            {
                return Json(new { success = false });
            }
        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {
            var courseQuery = from c in db.Courses
                              where c.Subject == subject && c.Num == number
                              select new
                              {
                                  id = c.CourseId
                              };

            if (courseQuery.Any())
            {

                var classQuery = from q in courseQuery
                                 join cl in db.Classes on q.id equals cl.CourseId
                                 where cl.Season == season && cl.Semester == year && cl.StartTime == TimeOnly.FromDateTime(start) && cl.EndTime == TimeOnly.FromDateTime(end) && cl.Location == location
                                 select cl;


                if (classQuery.Any())
                {
                    return Json(new { success = false });
                }

                try
                {
                    Class cla = new Class();
                    cla.StartTime = TimeOnly.FromDateTime(start);
                    cla.EndTime = TimeOnly.FromDateTime(end);
                    cla.Location = location;
                    cla.Season = season;
                    cla.Semester = (uint)year;
                    cla.Teacher = instructor;
                    cla.CourseId = courseQuery.First().id;
                    db.Add(cla);
                    db.SaveChanges();
                    return Json(new { success = true });
                } catch(Exception e)
                {
                    return Json(new { success = false });
                }

            }
            return Json(new { success = false });
        }


        /*******End code to modify********/

    }
}

