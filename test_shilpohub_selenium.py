
import argparse
import os
import unittest
from urllib.parse import parse_qs, urlparse
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.support.ui import WebDriverWait

BASE_URL = os.getenv('SHILPOHUB_URL', 'http://localhost:5173').rstrip('/')
BROWSER, HEADLESS, TIMEOUT = 'edge', False, 20


class BrowserTest(unittest.TestCase):
    def setUp(self):
        options = webdriver.ChromeOptions() if BROWSER == 'chrome' else webdriver.EdgeOptions()
        options.page_load_strategy = 'eager'
        if HEADLESS:
            options.add_argument('--headless=new')
        options.add_argument('--window-size=1440,1000')
        self.driver = (webdriver.Chrome if BROWSER == 'chrome' else webdriver.Edge)(options=options)
        self.addCleanup(self.driver.quit)
        self.driver.set_page_load_timeout(30)
        self.wait = WebDriverWait(self.driver, TIMEOUT)

    def open(self, path='/'):
        self.driver.get(BASE_URL + path)
        self.wait.until(EC.presence_of_element_located((By.ID, 'root')))

    def visible(self, by, locator):
        return self.wait.until(EC.visibility_of_element_located((by, locator)))

    def click(self, by, locator):
        self.wait.until(EC.element_to_be_clickable((by, locator))).click()

    def heading(self, text):
        return self.visible(By.XPATH, f"//h1[contains(normalize-space(.), '{text}')]")

    def path_is(self, path):
        self.wait.until(lambda d: urlparse(d.current_url).path == path)

    def registration_fields(self, confirmation='DemoPassword123!'):
        for field, value in [('register-name', 'Selenium Demo'), ('register-email', 'selenium@example.test'),
                             ('register-password', 'DemoPassword123!'), ('register-confirm', confirmation)]:
            self.visible(By.ID, field).send_keys(value)


class PublicUITests(BrowserTest):
    def test_01_homepage_and_footer(self):
        self.open()
        self.heading('Made by hand.')
        footer = self.driver.find_element(By.TAG_NAME, 'footer')
        self.driver.execute_script("arguments[0].scrollIntoView({block:'center'});", footer)
        self.assertIn('ShilpoHub', footer.text)
        self.assertGreater(len(footer.find_elements(By.TAG_NAME, 'a')), 5)

    def test_02_photographs_really_load(self):
        self.open()
        self.heading('Made by hand.')
        for name in ['pottery-photo', 'loom-photo', 'bangladesh-river',
                     'village-community', 'festival-community', 'learning-together']:
            with self.subTest(image=name):
                image = self.driver.find_element(By.CSS_SELECTOR, f'img[src$="{name}.jpg"]')
                self.driver.execute_script("arguments[0].scrollIntoView({block:'center'});", image)
                self.wait.until(lambda d: d.execute_script(
                    'return arguments[0].complete && arguments[0].naturalWidth > 0;', image))
                self.assertTrue(image.get_attribute('alt'))

    def test_03_lower_cards_use_different_photos(self):
        self.open()
        self.heading('Made by hand.')
        images = self.driver.find_elements(By.CSS_SELECTOR, 'main article img')
        self.assertEqual(len(images), 3)
        names = [image.get_attribute('src').rsplit('/', 1)[-1] for image in images]
        self.assertEqual(len(set(names)), 3)
        self.assertEqual(set(names), {'village-community.jpg', 'festival-community.jpg', 'learning-together.jpg'})

    def test_04_search_preserves_query(self):
        self.open()
        self.visible(By.CSS_SELECTOR, 'form[role="search"] input').send_keys('Jamdani & cotton')
        self.click(By.CSS_SELECTOR, 'form[role="search"] button[type="submit"]')
        self.path_is('/marketplace/products')
        self.assertEqual(parse_qs(urlparse(self.driver.current_url).query)['search'], ['Jamdani & cotton'])
        self.heading('All Products')
        self.assertEqual(self.driver.find_element(By.CSS_SELECTOR, 'input[type="search"]').get_attribute('value'), 'Jamdani & cotton')

    def test_05_registration_name_validation(self):
        self.open('/register')
        self.click(By.XPATH, "//button[normalize-space(.)='Continue']")
        self.visible(By.XPATH, "//*[normalize-space(.)='Enter your full name.']")
        self.assertTrue(self.driver.find_element(By.ID, 'register-name').is_displayed())

    def test_06_registration_password_mismatch(self):
        self.open('/register')
        self.registration_fields('Different123!')
        self.click(By.XPATH, "//button[normalize-space(.)='Continue']")
        self.visible(By.XPATH, "//*[normalize-space(.)='Passwords do not match.']")

    def test_07_registration_next_and_back(self):
        self.open('/register')
        self.registration_fields()
        self.click(By.XPATH, "//button[normalize-space(.)='Continue']")
        self.heading('Choose your account type')
        self.visible(By.XPATH, "//button[contains(.,'Producer')]")
        # Stop before final registration; no real account is created.
        self.click(By.XPATH, "//button[normalize-space(.)='Back']")
        self.assertEqual(self.visible(By.ID, 'register-name').get_attribute('value'), 'Selenium Demo')

    def test_08_login_fields_and_forgot_password_link(self):
        self.open('/login')
        self.heading('Welcome back')
        email = self.visible(By.ID, 'login-email')
        email.send_keys('first@example.test')
        email.clear()
        email.send_keys('second@example.test')
        self.assertEqual(email.get_attribute('value'), 'second@example.test')
        self.assertEqual(self.driver.find_element(By.ID, 'login-password').get_attribute('type'), 'password')
        self.click(By.LINK_TEXT, 'Forgot password?')
        self.path_is('/forgot-password')

    def test_09_producer_requires_login(self):
        self.open('/producer')
        self.path_is('/login')
        self.heading('Welcome back')

    def test_10_mobile_layout_and_menu(self):
        self.driver.set_window_size(390, 844)
        self.open()
        self.heading('Made by hand.')
        width, viewport = self.driver.execute_script('return [document.documentElement.scrollWidth, innerWidth];')
        self.assertLessEqual(width, viewport, 'Horizontal overflow on mobile')
        self.click(By.CSS_SELECTOR, 'button[aria-label="Toggle menu"]')
        self.click(By.XPATH, "//header//a[@href='/register']")
        self.path_is('/register')


@unittest.skipUnless(os.getenv('SHILPOHUB_PRODUCER_EMAIL') and os.getenv('SHILPOHUB_PRODUCER_PASSWORD'),
                     'Set producer test-account credentials to enable live producer checks')
class ProducerUITests(BrowserTest):
    def setUp(self):
        super().setUp()
        self.open('/login')
        self.visible(By.ID, 'login-email').send_keys(os.environ['SHILPOHUB_PRODUCER_EMAIL'])
        self.visible(By.ID, 'login-password').send_keys(os.environ['SHILPOHUB_PRODUCER_PASSWORD'])
        self.click(By.CSS_SELECTOR, 'form button[type="submit"]')
        self.path_is('/producer')
        self.heading('Producer Dashboard')

    def test_11_revenue_chart_or_explicit_empty_state(self):
        chart = self.visible(By.XPATH, "//section[.//h3[contains(.,'Revenue')]]")
        if chart.find_elements(By.TAG_NAME, 'svg'):
            self.assertGreater(len(chart.find_elements(By.TAG_NAME, 'circle')), 0)
            chart.find_element(By.TAG_NAME, 'summary').click()
            self.assertTrue(chart.find_element(By.TAG_NAME, 'table').is_displayed())
            self.assertGreater(len(chart.find_elements(By.CSS_SELECTOR, 'tbody tr')), 0)
        else:
            self.assertIn('No analytics data is available yet.', chart.text)
        # A failed API response is not treated as successful empty data.

    def test_12_growth_pages_load(self):
        for path, title in [('/producer/csr-sponsorship', 'CSR Sponsorship'),
                            ('/producer/investment-opportunities', 'Investment Opportunities')]:
            with self.subTest(page=path):
                self.open(path)
                self.heading(title)
                self.wait.until(lambda d: 'Loading' not in d.find_element(By.TAG_NAME, 'main').text)
                main = self.driver.find_element(By.TAG_NAME, 'main')
                self.assertNotIn('Unable to', main.text)
                self.assertFalse(main.find_elements(By.CSS_SELECTOR, '[role="alert"]'))
                self.assertTrue(main.find_elements(By.XPATH, ".//button[normalize-space(.)='Proposals']")
                                or "You haven't posted any" in main.text,
                                'Expected opportunity cards or an explicit empty state')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--url', default=BASE_URL)
    parser.add_argument('--browser', choices=['edge', 'chrome'], default='edge')
    parser.add_argument('--headless', action='store_true')
    parser.add_argument('--timeout', type=int, default=20)
    args, remaining = parser.parse_known_args()
    BASE_URL, BROWSER, HEADLESS, TIMEOUT = args.url.rstrip('/'), args.browser, args.headless, args.timeout
    if urlparse(BASE_URL).scheme not in ('http', 'https'):
        parser.error('--url must start with http:// or https://')
    unittest.main(argv=[__file__, *remaining], verbosity=2)
