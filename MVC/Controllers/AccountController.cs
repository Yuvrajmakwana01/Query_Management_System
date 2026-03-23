using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using Repository.Models;
using Repository.Services;

namespace MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountInterface _authRepo;

        private readonly RedisServices _redis;
        private readonly EmailServices _emailService;

        public AccountController(IAccountInterface authRepo, EmailServices emailService, RedisServices redis)
        {
            _authRepo = authRepo;
            _emailService = emailService;
            _redis = redis;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // 1. Check if Employee (Admin or Staff)
            var emp = await _authRepo.LoginEmployee(email, password);
            if (emp != null)
            {
                HttpContext.Session.SetInt32("UserId", emp.c_EmpId);
                HttpContext.Session.SetString("UserName", emp.c_EmpName ?? "");
                HttpContext.Session.SetString("UserRole", emp.c_Role ?? "employee");

                return Json(new { success = true, role = emp.c_Role });
            }

            // 2. Check if Regular User
            var user = await _authRepo.LoginUser(email, password);
            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.c_UserId);
                HttpContext.Session.SetString("UserName", user.c_UserName ?? "");
                Console.WriteLine(HttpContext.Session.GetString("UserName"));
                HttpContext.Session.SetString("UserRole", "user");

                return Json(new { success = true, role = "user" });
            }

            return Json(new { success = false, message = "Invalid email or password" });
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // 🔥 STEP 1: Forgot Password Page
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // 🔥 STEP 1: SEND OTP
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Json(new { success = false, message = "Please enter email" });

            email = email.Trim().ToLower();

            // 👉 Check email exists
            var userType = await _authRepo.CheckEmailExists(email);

            if (userType == null)
                return Json(new { success = false, message = "Email not registered" });

            // 👉 Generate OTP
            var otp = new Random().Next(100000, 999999).ToString();

            // 🔥 Save OTP in Redis (15 min expiry handled there)
            await _redis.SaveOtpAsync(email, otp, userType);

            // 🔥 Load HTML template
            string templatePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Templates",
                "ForgotPasswordOtp.html"
            );

            string template = await System.IO.File.ReadAllTextAsync(templatePath);

            // 🔥 Replace dynamic values
            template = template.Replace("{{OTP}}", otp);

            // 🔥 Logo path
            string logoPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "images",
                "QueryLogo.png"
            );

            // 🔥 Send email using your EmailService
            await _emailService.SendEmailWithLogo(
                email,
                "Password Reset OTP 🔐",
                template,
                logoPath
            );

            // 🔥 Save session for next steps
            HttpContext.Session.SetString("ResetEmail", email);
            HttpContext.Session.SetString("ResetUserType", userType);

            return Json(new { success = true, message = "OTP sent to your email" });
        }

        // 🔥 STEP 2: VERIFY OTP PAGE
        [HttpGet]
        public IActionResult VerifyOtp()
        {
            if (HttpContext.Session.GetString("ResetEmail") == null)
                return RedirectToAction("Login");

            return View();
        }

        // 🔥 STEP 2: VERIFY OTP
        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string email, string otp)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp))
                return Json(new { success = false, message = "Email and OTP required" });

            email = email.Trim().ToLower();

            var userType = await _redis.VerifyOtpAsync(email, otp);

            if (userType == null)
                return Json(new { success = false, message = "Invalid or expired OTP" });

            // 🔥 Mark OTP verified
            HttpContext.Session.SetString("OtpVerified", "true");

            return Json(new { success = true });
        }

        // 🔥 STEP 3: RESET PASSWORD PAGE
        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (HttpContext.Session.GetString("ResetEmail") == null)
                return RedirectToAction("Login");

            if (HttpContext.Session.GetString("OtpVerified") != "true")
                return RedirectToAction("ForgotPassword");

            return View();
        }

        // 🔥 STEP 3: RESET PASSWORD
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string newPassword, string confirmPassword)
        {
            var email = HttpContext.Session.GetString("ResetEmail");
            var userType = HttpContext.Session.GetString("ResetUserType");
            var otpVerified = HttpContext.Session.GetString("OtpVerified");

            if (string.IsNullOrEmpty(email) || otpVerified != "true")
                return Json(new { success = false, message = "Session expired" });

            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
                return Json(new { success = false, message = "Fill all fields" });

            if (newPassword != confirmPassword)
                return Json(new { success = false, message = "Passwords do not match" });

            if (newPassword.Length < 6)
                return Json(new { success = false, message = "Password must be at least 6 characters" });

            // 🔥 Update password
            var result = await _authRepo.ResetPassword(email, newPassword, userType ?? "user");

            if (!result)
                return Json(new { success = false, message = "Password update failed" });

            // 🔥 Clear session
            HttpContext.Session.Remove("ResetEmail");
            HttpContext.Session.Remove("ResetUserType");
            HttpContext.Session.Remove("OtpVerified");

            return Json(new { success = true, message = "Password reset successful" });
        }
    }
}