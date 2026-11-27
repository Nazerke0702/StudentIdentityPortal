import assert from "node:assert/strict";
import { mkdir } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { chromium } from "@playwright/test";

const scriptDir = path.dirname(fileURLToPath(import.meta.url));
const projectDir = path.resolve(scriptDir, "..");
const evidenceDir = path.join(projectDir, "docs", "screenshots");
const baseUrl = process.env.PORTAL_BASE_URL ?? "http://127.0.0.1:5188";
const adminEmail = process.env.PORTAL_ADMIN_EMAIL;
const adminPassword = process.env.PORTAL_ADMIN_PASSWORD;

assert(adminEmail, "PORTAL_ADMIN_EMAIL must be supplied by the PowerShell runner.");
assert(adminPassword, "PORTAL_ADMIN_PASSWORD must be supplied by the PowerShell runner.");
await mkdir(evidenceDir, { recursive: true });

const browser = await chromium.launch({ headless: true });
try {
  const context = await browser.newContext({ viewport: { width: 1440, height: 1000 } });
  const page = await context.newPage();
  const capture = (name) => page.screenshot({
    path: path.join(evidenceDir, name),
    fullPage: true
  });
  const visit = (route) => page.goto(new URL(route, baseUrl).toString(), { waitUntil: "networkidle" });
  const submit = async () => page.getByRole("button", { name: /Create student account/ }).click();

  await visit("/");
  await capture("01-home.png");

  await visit("/Account/Register");
  await capture("02-register.png");
  await page.getByLabel("First name").fill("Evidence");
  await page.getByLabel("Last name").fill("Student");
  await page.getByLabel("University email").fill("invalid-" + Date.now() + "@example.test");
  await page.locator("#Password").fill("Campus!Student2026");
  await page.locator("#ConfirmPassword").fill("Different!Student2026");
  await submit();
  await page.locator('[data-valmsg-for="ConfirmPassword"]').waitFor();
  assert.match(await page.locator('[data-valmsg-for="ConfirmPassword"]').innerText(), /match/i);
  await capture("03-register-validation.png");

  const studentEmail = "evidence-student-" + Date.now() + "@example.test";
  await visit("/Account/Register");
  await page.getByLabel("First name").fill("Evidence");
  await page.getByLabel("Last name").fill("Student");
  await page.getByLabel("University email").fill(studentEmail);
  await page.locator("#Password").fill("Campus!Student2026");
  await page.locator("#ConfirmPassword").fill("Campus!Student2026");
  await submit();
  await page.waitForURL(/\/Dashboard(?:$|\?)/);
  await capture("04-register-success.png");

  await page.getByRole("button", { name: "Sign out" }).click();
  await page.waitForURL((url) => url.pathname === "/" || url.pathname === "/Home/Index");
  await visit("/Account/Login");
  await capture("05-login.png");
  await page.getByLabel("University email").fill(studentEmail);
  await page.locator("#Password").fill("Wrong!Password2026");
  await page.getByRole("button", { name: /Sign in to your account/ }).click();
  await page.getByText("The email or password was not recognized.").waitFor();
  await capture("06-login-validation.png");

  await page.getByLabel("University email").fill(studentEmail);
  await page.locator("#Password").fill("Campus!Student2026");
  await page.getByRole("button", { name: /Sign in to your account/ }).click();
  await page.waitForURL(/\/Dashboard(?:$|\?)/);
  await capture("07-student-dashboard.png");
  await visit("/Admin");
  await page.waitForURL(/\/Account\/AccessDenied/);
  await capture("08-student-access-denied.png");

  await page.getByRole("button", { name: "Sign out" }).click();
  await visit("/Account/Login");
  await page.getByLabel("University email").fill(adminEmail);
  await page.locator("#Password").fill(adminPassword);
  await page.getByRole("button", { name: /Sign in to your account/ }).click();
  await page.waitForURL(/\/Dashboard(?:$|\?)/);
  await visit("/Admin");
  await page.getByText("PORTAL MANAGEMENT").waitFor();
  await capture("09-admin-dashboard.png");
  await page.getByRole("button", { name: "Remove" }).first().waitFor();
  await capture("10-admin-protected-action.png");

  await page.getByRole("button", { name: "Sign out" }).click();
  await page.waitForURL((url) => url.pathname === "/" || url.pathname === "/Home/Index");
  await capture("11-logout.png");
  await context.close();
} finally {
  await browser.close();
}
