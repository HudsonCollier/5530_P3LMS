﻿// Implemented by Hudson Collier and Ian Kerr
// Last Edited: April 16, 2025

using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from e in db.Enrolleds
                        join cl in db.Classes on e.ClassId equals cl.ClassId
                        join c in db.Courses on cl.CourseId equals c.CourseId
                        where e.UId == uid
                        select new
                        {
                            subject = c.Subject,
                            number = c.Num,
                            name = c.Name,
                            season = cl.Season,
                            year = cl.Semester,
                            grade = e.Grade == null ? "--" : e.Grade
                        };
      
                return Json(query.ToList());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            // get all assignments, then check if each assignment has submission
            var query = from e in db.Enrolleds
                        join st in db.Students on e.UId equals st.UId
                        join cl in db.Classes on e.ClassId equals cl.ClassId
                        join c in db.Courses on cl.CourseId equals c.CourseId
                        join ac in db.AssignmentCategories on cl.ClassId equals ac.ClassId
                        join a in db.Assignments on ac.CategoryId equals a.CategoryId
                        where e.UId == uid && c.Subject == subject && c.Num == num && cl.Season == season && cl.Semester == year
                        select new
                        {
                            assign = a,
                            assignCat = ac,
                            
                        };
            var query2 = from q in query
                         join s in db.Submissions
                         on new { A = q.assign.AssignId, B = uid } equals new { A = s.AssignId, B = s.UId }
                         into joined
                         from j in joined.DefaultIfEmpty()
                         select new { 
                             score = j.Score,
                             aname = q.assign.Name,
                             cname = q.assignCat.Name,
                             due = q.assign.Due,
                         };


            return Json(query2.ToArray());
        }

        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            var query = from e in db.Enrolleds
                        join cl in db.Classes on e.ClassId equals cl.ClassId
                        join c in db.Courses on cl.CourseId equals c.CourseId
                        join ac in db.AssignmentCategories on cl.ClassId equals ac.ClassId
                        join a in db.Assignments on ac.CategoryId equals a.CategoryId
                        where e.UId == uid && c.Subject == subject && c.Num == num && cl.Season == season && cl.Semester == year && ac.Name == category && a.Name == asgname
                        select a;
            
            var assignment = query.FirstOrDefault();
            if (assignment == null)
            {
                return Json(new { success = false });
            }

            var subQuery = from q in query
                           join sb in db.Submissions on q.AssignId equals sb.AssignId
                           where sb.UId == uid
                           select sb;

            var submit = subQuery.FirstOrDefault();
            if (submit != null)
            {
                db.Submissions.Remove(submit);
                db.SaveChanges();
            }

            Submission s = new Submission();
            s.UId = uid;
            s.AssignId = assignment.AssignId;
            s.Time = DateTime.Now;
            s.Contents = contents;
            db.Submissions.Add(s);
            db.SaveChanges();

            return Json(new { success = true });
        }

        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            var query = from c in db.Courses join cl in db.Classes on c.CourseId equals cl.CourseId
                        where c.Subject == subject && c.Num == num && cl.Season == season && cl.Semester == year
                        select new
                        {
                            id = cl.ClassId
                        };

            var targetClass = query.FirstOrDefault();

            if (targetClass == null)
            {
                return Json(new { success = false });
            }

            var checkEnrollment = from en in db.Enrolleds
                                  where en.ClassId == targetClass.id && en.UId == uid
                                  select en;

            if (checkEnrollment.FirstOrDefault() != null)
            {
                return Json(new { success = false });
            }
            Enrolled e = new Enrolled();
            e.UId = uid;
            e.ClassId = query.First().id;
            db.Enrolleds.Add(e);
            db.SaveChanges();
            return Json(new { success = true });
        }

        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            var query = from e in db.Enrolleds
                        where e.UId == uid && e.Grade != null
                        select new
                        {
                            e.Grade,
                        };

            double runningGPA = 0;
            int classCount = 0;

            foreach (var grade in query.ToList())
            {
                runningGPA += ConvertToGPA(grade.Grade);
                classCount++;
            }

            double gpa = classCount > 0 ? runningGPA / classCount : 0.0;

            return Json(new { gpa = Math.Round(gpa, 2) });
        }
        

        /// <summary>
        /// Converts the letter grade to the corresponding GPA
        /// </summary>
        /// <param name="letterGrade"></param>
        /// <returns></returns>
        private double ConvertToGPA(string letterGrade)
        {
            if (letterGrade == "A") return 4.0;
            else if (letterGrade == "A-") return 3.7;
            else if (letterGrade == "B+") return 3.3;
            else if (letterGrade == "B") return 3.0;
            else if (letterGrade == "B-") return 2.7;
            else if (letterGrade == "C+") return 2.3;
            else if (letterGrade == "C") return 2.0;
            else if (letterGrade == "C-") return 1.7;
            else if (letterGrade == "D+") return 1.3;
            else if (letterGrade == "D") return 1.0;
            else if (letterGrade == "D-") return 0.7;
            else return 0.0;
        }
        /*******End code to modify********/
    }
}

