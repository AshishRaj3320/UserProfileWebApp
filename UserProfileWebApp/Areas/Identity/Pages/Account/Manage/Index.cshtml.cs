// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UserProfileWebApp.Models;
using UserProfileWebApp.Validators;

namespace UserProfileWebApp.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Display(Name = "Last Name")]
            [Required]
            public string LastName { get; set; }


            [Display(Name = "Address")]
            [Required]
            public string Address { get; set; }


            [Display(Name = "Profile Picture")]
            [DataType(DataType.Upload)]
            [MaxFileSize(1024 * 1024, ErrorMessage = "Profile photo size must be less than 1 MB.")]
            [FileExtension(new[] { ".jpg", ".jpeg", ".png" }, ErrorMessage = "Please upload profile with jpg, jpeg, or png extension.")]
            public IFormFile ProfilePicture { get; set; }

            public string ProfilePicturePath { get; set; }

            [Display(Name = "Resume")]
            [DataType(DataType.Upload)]
            [MaxFileSize(2048 * 1024, ErrorMessage = "Resume file size must be less than 2 MB.")]
            [FileExtension(new[] { ".doc", ".docx", ".pdf" }, ErrorMessage = "Please upload resume with pdf, doc, or docx extension.")]
            public IFormFile Resume { get; set; }

            public string ResumePath { get; set; }

            public bool IsProfilePictureExists { get; set; }
            public bool IsResumeExists { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {

            Username = user.UserName;

            Input = new InputModel
            {
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address,
                ProfilePicturePath = user.ProfilePicturePath,
                ResumePath=user.UserFiles,
                IsProfilePictureExists = !string.IsNullOrEmpty(user.ProfilePicturePath),
                IsResumeExists = !string.IsNullOrEmpty(user.UserFiles)
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // Handle Profile Picture Upload
            if (Input.ProfilePicture != null && Input.ProfilePicture.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads"); // Adjust the path as necessary
                var filePath = Path.Combine(uploads, Input.ProfilePicture.FileName);

                // Ensure the uploads directory exists
                Directory.CreateDirectory(uploads);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.ProfilePicture.CopyToAsync(fileStream);
                }

                // Set the path to the profile picture
                Input.ProfilePicturePath = "/uploads/" + Input.ProfilePicture.FileName; // Set the relative path
            }

            // Handle Resume Upload
            if (Input.Resume != null && Input.Resume.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads"); // Adjust the path as necessary
                var resumePath = Path.Combine(uploads, Input.Resume.FileName);

                // Ensure the uploads directory exists
                Directory.CreateDirectory(uploads);

                using (var fileStream = new FileStream(resumePath, FileMode.Create))
                {
                    await Input.Resume.CopyToAsync(fileStream);
                }

                // Set the path to the resume
                Input.ResumePath = "/uploads/" + Input.Resume.FileName; // Set the relative path
            }

            // Update user properties
            user.PhoneNumber = Input.PhoneNumber;
            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;
            user.Address = Input.Address;
            if (Input.ProfilePicture != null)
            {
                user.ProfilePicturePath = Input.ProfilePicturePath;
            }
            if (Input.Resume != null)
            {
                user.UserFiles = Input.ResumePath;
            }

            var setPhoneResult = await _userManager.UpdateAsync(user);
            if (!setPhoneResult.Succeeded)
            {
                StatusMessage = "Unexpected error when trying to set phone number.";
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
