# ShilpoHubBD Selenium tests

## What is the testing in Week 8?
**Selenium WebDriver automated functional UI testing with Python.** The sample scripts open Edge/Chrome, locate elements using ID, CSS, XPath or link text, enter data, click controls and assert results. This is browser/system testing, not Python unit testing of individual business functions.

Our script uses the same Selenium operations, with Python's built-in unittest runner for named test cases and pass/fail output. Explicit waits replace fixed sleeps; browsers always close after each test.

## Files (directly in ShilpoHubBD, outside all subfolders)
- test_shilpohub_selenium.py — 12 test cases.
- requirements-selenium.txt — Selenium dependency.
- SELENIUM_TESTING.md — these instructions.

## Run from PowerShell
Start the frontend in one terminal:
```powershell
cd 'C:\Users\smart view\OneDrive\Desktop\ShilpoHubBD\frontend'
npm run dev
```
In another terminal:
```powershell
cd 'C:\Users\smart view\OneDrive\Desktop\ShilpoHubBD'
py -m pip install -r requirements-selenium.txt
py test_shilpohub_selenium.py
```
Microsoft Edge is the default. Selenium Manager obtains the matching driver (internet needed on first run). Chrome is also supported:
```powershell
py test_shilpohub_selenium.py --browser chrome --headless
```
Use the actual URL printed by Vite if the port differs:
```powershell
py test_shilpohub_selenium.py --url http://localhost:5174 --headless
```
Run just one test:
```powershell
py test_shilpohub_selenium.py --headless PublicUITests.test_04_search_preserves_query
```

## Coverage
1. Homepage heading and footer links.
2. All six real homepage photographs decode successfully.
3. The lower three cards use distinct photos, not the upper row's images.
4. Search navigation preserves special characters and input text.
5. Registration rejects an empty name.
6. Registration rejects mismatched passwords.
7. Registration Next/Back preserves data, stopping before account creation.
8. Login field input/clear and forgot-password navigation.
9. Anonymous visitors cannot enter the producer dashboard.
10. Mobile layout avoids horizontal overflow and navigation works.
11. Optional live producer revenue chart/data table or explicit empty state.
12. Optional live Growth pages render opportunities or their empty messages.

Tests 1–10 exercise frontend behavior and do not prove backend functionality. API error messages may be present while these frontend-only checks pass. Tests 11–12 require a running backend and an existing producer test account; they fail on API errors instead of treating errors as empty data.

To enable optional tests, set SHILPOHUB_PRODUCER_EMAIL and SHILPOHUB_PRODUCER_PASSWORD in your terminal environment. Keep real passwords out of the test source. Without both variables, the two tests report SKIPPED (not PASSED). These tests only sign in and view pages; they do not approve proposals or create products/orders.

## Reading results
- ok: the assertions passed.
- FAIL: the observed UI did not match its expected behavior.
- ERROR: setup, browser, element lookup or timeout failed; inspect the traceback.
- skipped: producer credentials were not supplied.
- Exit code 0: no failures/errors; skips may still be present.

If Selenium cannot find Edge/Chrome, install one of those browsers. If the page cannot be reached, start Vite and check --url. If backend-dependent tests fail, start the backend's HTTP profile on localhost:5065 and verify the producer account.

Official references:
- https://www.selenium.dev/documentation/webdriver/getting_started/first_script/
- https://www.selenium.dev/documentation/webdriver/waits/

## Verification in this session
Python syntax parsed successfully and all 12 test methods were discovered. A live Selenium run was attempted with both Edge and Chrome. Edge disconnected from WebDriver and Chrome failed to start, before UI assertions could complete. Therefore no browser test pass is claimed. The two optional producer tests were skipped because test credentials were not provided. Run the commands above in your normal terminal to verify against your running application.
