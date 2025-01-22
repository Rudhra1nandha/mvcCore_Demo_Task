
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using mvccore_dotnet_app.Models;
using mvccore_dotnet_app.Data;
using System.Data;
//excel
using OfficeOpenXml;
//pdf
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.IO;
using iText.IO.Font.Constants;
using System.Linq;


using iText.Layout.Properties;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;



namespace mvccore_dotnet_app.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            var model = new UserRole(); // Initialize a new User object
            return View(model);
        }

        [HttpPost]
        public IActionResult Register(UserRole model)
        {
            if (ModelState.IsValid)
            {
                _context.UserRole.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            var model = new UserRole(); // Initialize a new User object
            return View();
        }



        [HttpGet]
        public IActionResult Dashboard(string username, string password, string role)
        {
            var user = _context.UserRole.FirstOrDefault(u => u.Email == username && u.Password == password);
            if (user != null)
            {
                ViewBag.Role = user.Role;

                // Retrieve all employees with "Employee" role
                var employees = _context.UserRole.Where(a => a.Role == "Employee").ToList();

                // Group employees by rank and count them
                var rankDistribution = employees
                    .GroupBy(e => e.Rank)
                    .ToDictionary(g => g.Key?.ToString() ?? "Unspecified", g => g.Count());

                // Pass chart data to the view
                ViewData["EmployeeRankDistribution"] = rankDistribution;

                if (user.Role == "Admin")
                {
                    return View(employees); // Show employees to admin
                }
                else
                {
                    var admin = _context.UserRole.Where(a => a.Role == "Admin").ToList();
                    return View(admin); // Show admin data for other roles
                }
            }
            else
            {
                ViewBag.Error = "Invalid credentials";
                return RedirectToAction("Index", "Home");
            }
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _context.UserRole.FirstOrDefault(u => u.id == id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user); // Render an Edit view for the user
        }

        [HttpPost]
        public IActionResult Edit(UserRole model)
        {
            var user = _context.UserRole.FirstOrDefault(u => u.id == model.id);
            if (user != null)
            {
                user.UserName = model.UserName;
                user.Email = model.Email;
                user.Phone = model.Phone;
                user.Role = model.Role;
                
                
                    user.Rank = model.Rank;
                
                user.DateOfBirth = model.DateOfBirth;
                user.Native = model.Native;
                user.Pincode = model.Pincode;

                _context.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var user = _context.UserRole.FirstOrDefault(u => u.id == id);
            if (user != null)
            {
                _context.UserRole.Remove(user);
                _context.SaveChanges();
            }
            return RedirectToAction("Dashboard");
        }

        //--------------excel-----------------
        public IActionResult ExportToExcel(string role)
        {
            // Set the license context to non-commercial
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Toggle between "Admin" and "Employee" roles
            if (role == "Admin")
            {
                role = "Employee"; // Change role to "Employee"
            }
            else if (role == "Employee")
            {
                role = "Admin"; // Change role to "Admin"
            }

            // Filter the users by the role passed in the query string
            var employees = _context.UserRole.Where(a => a.Role == role).ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Users");
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Name";
                worksheet.Cells[1, 3].Value = "Email";
                worksheet.Cells[1, 4].Value = "Mobile No";
                worksheet.Cells[1, 5].Value = "Role";
                worksheet.Cells[1, 6].Value = "Date of Birth";
                worksheet.Cells[1, 7].Value = "Native Place";
                worksheet.Cells[1, 8].Value = "Pincode";

                int row = 2;
                foreach (var user in employees)
                {
                    worksheet.Cells[row, 1].Value = user.id;
                    worksheet.Cells[row, 2].Value = user.UserName;
                    worksheet.Cells[row, 3].Value = user.Email;
                    worksheet.Cells[row, 4].Value = user.Phone;
                    worksheet.Cells[row, 5].Value = user.Role;
                    worksheet.Cells[row, 6].Value = user.DateOfBirth;
                    worksheet.Cells[row, 7].Value = user.Native;
                    worksheet.Cells[row, 8].Value = user.Pincode;
                    row++;
                }

                var stream = new MemoryStream(package.GetAsByteArray());
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Users.xlsx");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Message = "Please select a valid Excel file.";
                return RedirectToAction("Dashboard");
            }

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);

                    using (var package = new OfficeOpenXml.ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            ViewBag.Message = "Excel file is empty.";
                            return RedirectToAction("Dashboard");
                        }

                        int rowCount = worksheet.Dimension.Rows;
                        List<UserRole> users = new List<UserRole>();

                        for (int row = 2; row <= rowCount; row++) // Assuming row 1 is the header
                        {
                            var user = new UserRole
                            {
                                UserName = worksheet.Cells[row, 1].Text,
                                Email = worksheet.Cells[row, 2].Text,
                                Phone = Convert.ToInt64(worksheet.Cells[row, 3].Text),
                                Role = worksheet.Cells[row, 4].Text,
                                DateOfBirth = worksheet.Cells[row, 5].Text,
                                Native = worksheet.Cells[row, 6].Text,
                                Pincode = worksheet.Cells[row, 7].Text,
                                Password = worksheet.Cells[row, 8].Text,
                                Cpassword = worksheet.Cells[row, 9].Text
                            };

                            users.Add(user);
                        }

                        // Add users to the database
                        _context.UserRole.AddRange(users);
                        await _context.SaveChangesAsync();
                        ViewBag.Message = "Excel data uploaded successfully.";
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error: {ex.Message}";
            }

            return RedirectToAction("Dashboard");
        }

    }
}
