import { test as setup, expect } from "@playwright/test";

setup("authenticate as admin", async ({ page }) => {
  await page.goto("/login");

  // M10 & M12 baseline LoginRequest uses Email
  // (with Username supported as fallback).
  await page
    .getByLabel(/email|username/i)
    .fill(
      process.env.TMS_ADMIN_EMAIL ??
        process.env.TMS_ADMIN_USER!
    );

  await page
    .getByLabel("Password")
    .fill(process.env.TMS_ADMIN_PASS!);

  await page
    .getByRole("button", { name: "Sign In" })
    .click();

  // M9 InstructorDashboardComponent renders
  // "Instructor Command Center".
  // This confirms that login successfully reached
  // the protected dashboard.
  await expect(
    page.getByRole("heading", {
      name: /command center/i,
    })
  ).toBeVisible();

  // Save authenticated browser session.
  await page.context().storageState({
    path: "playwright/.auth/admin.json",
  });
});