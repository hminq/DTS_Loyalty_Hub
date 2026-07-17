const form = document.querySelector("#login-form");
const passwordInput = document.querySelector("#password");
const togglePasswordButton = document.querySelector("#toggle-password");
const demoMessage = document.querySelector("#demo-message");

togglePasswordButton.addEventListener("click", () => {
  const shouldShowPassword = passwordInput.type === "password";
  passwordInput.type = shouldShowPassword ? "text" : "password";
  togglePasswordButton.textContent = shouldShowPassword ? "Hide" : "Show";
  togglePasswordButton.setAttribute(
    "aria-label",
    shouldShowPassword ? "Hide password" : "Show password"
  );
});

form.addEventListener("submit", (event) => {
  event.preventDefault();
  demoMessage.textContent = "Authentication will be connected later.";
});
