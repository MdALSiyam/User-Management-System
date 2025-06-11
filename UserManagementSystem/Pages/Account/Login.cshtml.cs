using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using UserManagementSystem.Services;

namespace UserManagementSystem.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly SignInManager<IdentityUser> _signInManager;

        public LoginModel(IUserService userService, SignInManager<IdentityUser> signInManager)
        {
            _userService = userService;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string Message { get; set; }
        public bool IsSuccess { get; set; }
        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = Url.Content("/Users/Index");
            }

            if (ModelState.IsValid)
            {
                var result = await _userService.LoginUserAsync(Input.Email, Input.Password, Input.RememberMe);

                if (result.Succeeded)
                {
                    Message = "Login successful!";
                    IsSuccess = true;
                    return LocalRedirect(returnUrl);
                }
                else if (result.IsLockedOut)
                {
                    Message = "User account locked out.";
                    IsSuccess = false;
                }
                else if (result.IsNotAllowed)
                {
                    Message = "This account is blocked.";
                    IsSuccess = false;
                }
                else
                {
                    Message = "Invalid login attempt.";
                    IsSuccess = false;
                }
            }

            return Page();
        }
    }
}