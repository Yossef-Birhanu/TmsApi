import { test, expect } from "@playwright/test";

test("admin approves a pending enrollment", async ({ page }) => {
  await page.goto("/dashboard");

  // M9 InstructorDashboardComponent renders
  // "Instructor Command Center".
  await expect(
    page.getByRole("heading", {
      name: /command center/i,
    })
  ).toBeVisible();

  // M9 EnrollmentListComponent renders an "Approve"
  // button when the enrollment is Pending.
  const firstApprove = page
    .getByRole("button", { name: "Approve" })
    .first();

  await firstApprove.click();

  // The status should change to Approved.
  await expect(
    page.getByText("Approved").first()
  ).toBeVisible();
});