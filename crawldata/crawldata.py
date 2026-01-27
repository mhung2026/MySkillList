import requests
from bs4 import BeautifulSoup
import json
import time
from datetime import datetime
from typing import List, Dict, Optional
import logging

# Setup logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)


class CourseraScraper:
    """Scraper for collecting course information from Coursera"""

    def __init__(self):
        self.base_url = "https://www.coursera.org"
        self.headers = {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36',
            'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8',
            'Accept-Language': 'en-US,en;q=0.5',
        }
        self.session = requests.Session()
        self.session.headers.update(self.headers)

    def search_courses(self, query: str, max_results: int = 20) -> List[str]:
        """
        Search for courses on Coursera and return course URLs

        Args:
            query: Search term for courses
            max_results: Maximum number of course URLs to return

        Returns:
            List of course URLs
        """
        search_url = f"{self.base_url}/search?query={query.replace(' ', '%20')}"
        logger.info(f"Searching for courses: {query}")

        try:
            response = self.session.get(search_url, timeout=10)
            response.raise_for_status()

            soup = BeautifulSoup(response.content, 'html.parser')
            course_links = []

            # Find all course links
            for link in soup.find_all('a', href=True):
                href = link['href']
                if '/learn/' in href and href not in course_links:
                    if href.startswith('http'):
                        course_links.append(href)
                    else:
                        course_links.append(f"{self.base_url}{href}")

                    if len(course_links) >= max_results:
                        break

            logger.info(f"Found {len(course_links)} course URLs")
            return course_links

        except requests.RequestException as e:
            logger.error(f"Error searching courses: {e}")
            return []

    def extract_course_info(self, course_url: str) -> Optional[Dict]:
        """
        Extract detailed information from a course page

        Args:
            course_url: URL of the course page

        Returns:
            Dictionary containing course information
        """
        logger.info(f"Extracting data from: {course_url}")

        try:
            response = self.session.get(course_url, timeout=10)
            response.raise_for_status()
            soup = BeautifulSoup(response.content, 'html.parser')

            course_data = {
                'url': course_url,
                'scraped_at': datetime.now().isoformat(),
                'title': self._extract_title(soup),
                'instructor': self._extract_instructor(soup),
                'organization': self._extract_organization(soup),
                'description': self._extract_description(soup),
                'rating': self._extract_rating(soup),
                'reviews_count': self._extract_reviews_count(soup),
                'enrollment_count': self._extract_enrollment_count(soup),
                'duration': self._extract_duration(soup),
                'level': self._extract_level(soup),
                'language': self._extract_language(soup),
                'subtitles': self._extract_subtitles(soup),
                'skills': self._extract_skills(soup),
                'syllabus': self._extract_syllabus(soup),
                'prerequisites': self._extract_prerequisites(soup),
                'price': self._extract_price(soup),
                'certificate': self._extract_certificate(soup),
            }

            logger.info(f"Successfully extracted: {course_data.get('title', 'Unknown')}")
            return course_data

        except requests.RequestException as e:
            logger.error(f"Error extracting course info from {course_url}: {e}")
            return None

    def _extract_title(self, soup: BeautifulSoup) -> str:
        """Extract course title"""
        title_tag = soup.find('h1')
        return title_tag.get_text(strip=True) if title_tag else "N/A"

    def _extract_instructor(self, soup: BeautifulSoup) -> List[str]:
        """Extract instructor names"""
        instructors = []
        instructor_elements = soup.find_all(class_=['instructor', 'Instructor'])

        for elem in instructor_elements:
            name = elem.get_text(strip=True)
            if name and name not in instructors:
                instructors.append(name)

        return instructors if instructors else ["N/A"]

    def _extract_organization(self, soup: BeautifulSoup) -> str:
        """Extract organization/university name"""
        org_tag = soup.find(['span', 'div', 'a'], class_=['partner', 'Partner'])
        return org_tag.get_text(strip=True) if org_tag else "N/A"

    def _extract_description(self, soup: BeautifulSoup) -> str:
        """Extract course description - tries multiple selectors"""
        # Try multiple common description selectors
        description_selectors = [
            # Class-based selectors
            {'name': ['div', 'p', 'section'], 'class_': ['description', 'Description']},
            {'name': ['div', 'p'], 'class_': ['about', 'About']},
            {'name': ['div', 'p'], 'class_': ['overview', 'Overview']},
            {'name': ['div'], 'class_': ['content', 'Content']},
            {'name': ['div'], 'class_': ['course-description']},
            # Meta description
            {'name': 'meta', 'attrs': {'name': 'description'}},
            {'name': 'meta', 'attrs': {'property': 'og:description'}},
        ]

        for selector in description_selectors:
            if 'attrs' in selector:
                desc_tag = soup.find(selector['name'], attrs=selector['attrs'])
                if desc_tag and desc_tag.get('content'):
                    return desc_tag.get('content').strip()
            else:
                desc_tag = soup.find(selector['name'], class_=selector['class_'])
                if desc_tag:
                    text = desc_tag.get_text(strip=True)
                    if len(text) > 50:  # Ensure it's substantial content
                        return text

        # Fallback: look for any paragraph with substantial text
        all_paragraphs = soup.find_all('p')
        for p in all_paragraphs:
            text = p.get_text(strip=True)
            if len(text) > 100:  # Look for longer paragraphs
                return text

        return "N/A"

    def _extract_rating(self, soup: BeautifulSoup) -> Optional[float]:
        """Extract course rating"""
        rating_tag = soup.find(['span', 'div'], class_=['rating', 'Rating'])
        if rating_tag:
            try:
                rating_text = rating_tag.get_text(strip=True)
                return float(rating_text.split()[0])
            except (ValueError, IndexError):
                pass
        return None

    def _extract_reviews_count(self, soup: BeautifulSoup) -> Optional[int]:
        """Extract number of reviews"""
        reviews_tag = soup.find(['span', 'div'], class_=['reviews', 'Reviews'])
        if reviews_tag:
            try:
                reviews_text = reviews_tag.get_text(strip=True)
                return int(''.join(filter(str.isdigit, reviews_text)))
            except ValueError:
                pass
        return None

    def _extract_enrollment_count(self, soup: BeautifulSoup) -> Optional[int]:
        """Extract enrollment count"""
        enrollment_tag = soup.find(['span', 'div'], class_=['enrollment', 'Enrollment'])
        if enrollment_tag:
            try:
                enrollment_text = enrollment_tag.get_text(strip=True)
                return int(''.join(filter(str.isdigit, enrollment_text)))
            except ValueError:
                pass
        return None

    def _extract_duration(self, soup: BeautifulSoup) -> str:
        """Extract course duration"""
        duration_tag = soup.find(['span', 'div'], string=lambda text: text and ('hour' in text.lower() or 'week' in text.lower()))
        return duration_tag.get_text(strip=True) if duration_tag else "N/A"

    def _extract_level(self, soup: BeautifulSoup) -> str:
        """Extract difficulty level"""
        level_tag = soup.find(['span', 'div'], string=lambda text: text and any(lvl in text.lower() for lvl in ['beginner', 'intermediate', 'advanced']))
        return level_tag.get_text(strip=True) if level_tag else "N/A"

    def _extract_language(self, soup: BeautifulSoup) -> str:
        """Extract course language"""
        lang_tag = soup.find(['span', 'div'], class_=['language', 'Language'])
        return lang_tag.get_text(strip=True) if lang_tag else "N/A"

    def _extract_subtitles(self, soup: BeautifulSoup) -> List[str]:
        """Extract available subtitles"""
        subtitle_tags = soup.find_all(['span', 'div'], class_=['subtitle', 'Subtitle'])
        return [tag.get_text(strip=True) for tag in subtitle_tags] if subtitle_tags else []

    def _extract_skills(self, soup: BeautifulSoup) -> List[str]:
        """Extract skills taught in the course - focuses on 'Skills you'll gain' section"""
        skills = []

        # Primary Strategy: Look for Coursera's specific skill link pattern
        # Skills are typically in <a> tags with classes like "cds-119 cds-113 cds-115"
        skill_links = soup.find_all('a', class_=lambda x: x and 'cds-119' in str(x))

        for link in skill_links:
            skill = link.get_text(strip=True)
            # Filter out non-skill links
            if (skill and
                3 < len(skill) < 100 and
                skill not in skills and
                not any(x in skill.lower() for x in [
                    'enroll', 'sign in', 'sign up', 'learn more', 'explore',
                    'certificate', 'start', 'join', 'browse', 'about',
                    'status:', 'free trial', 'coursera'
                ])):
                # Check if this link is near "skills" section
                parent_section = link.find_parent(['div', 'section'])
                if parent_section:
                    section_text = parent_section.get_text()[:500].lower()
                    # Only add if near skills section or in a reasonable context
                    if ('skill' in section_text or
                        'you\'ll gain' in section_text or
                        'you will learn' in section_text or
                        len(skills) < 15):  # Or just collect first 15 reasonable ones
                        skills.append(skill)

        # Fallback Strategy 1: Look for "Skills you'll gain" section header
        if len(skills) < 3:
            skills_section = soup.find(string=lambda text: text and "skills you'll gain" in text.lower())
            if skills_section:
                # Find the parent container
                parent = skills_section.find_parent(['div', 'section', 'h2', 'h3'])
                if parent:
                    # Look for next sibling containing skills
                    container = parent.find_next_sibling(['div', 'ul'])
                    if not container:
                        # Try parent's parent approach
                        container = parent.parent

                    if container:
                        # Look for all links and buttons in this section
                        skill_elements = container.find_all(['a', 'span', 'button'], limit=30)
                        for elem in skill_elements:
                            skill = elem.get_text(strip=True)
                            if (skill and
                                3 < len(skill) < 100 and
                                skill not in skills and
                                skill.lower() not in ["skills you'll gain", "skills", "more", "less", "show more", "show less"] and
                                not any(x in skill.lower() for x in ['status:', 'free trial', 'enroll'])):
                                skills.append(skill)

        # Fallback Strategy 2: Look for structured data (JSON-LD)
        if len(skills) < 3:
            json_ld_scripts = soup.find_all('script', type='application/ld+json')
            for script in json_ld_scripts:
                try:
                    if script.string:
                        data = json.loads(script.string)
                        # Look for skills in various possible fields
                        if isinstance(data, dict):
                            for key in ['teaches', 'skills', 'about', 'educationalCredentialAwarded']:
                                if key in data:
                                    skill_data = data[key]
                                    if isinstance(skill_data, list):
                                        for item in skill_data:
                                            if isinstance(item, str):
                                                skills.append(item)
                                            elif isinstance(item, dict) and 'name' in item:
                                                skills.append(item['name'])
                                    elif isinstance(skill_data, str):
                                        skills.append(skill_data)
                except (json.JSONDecodeError, AttributeError, TypeError):
                    continue

        # Clean up and deduplicate
        cleaned_skills = []
        seen_lower = set()

        for skill in skills:
            skill = skill.strip()
            skill_lower = skill.lower()

            if (skill and
                3 < len(skill) < 100 and
                skill_lower not in seen_lower and
                not any(x in skill_lower for x in [
                    'status:', 'free trial', 'enroll', 'certificate',
                    'learn more', 'coursera', 'sign in', 'sign up'
                ])):
                cleaned_skills.append(skill)
                seen_lower.add(skill_lower)

                if len(cleaned_skills) >= 15:  # Limit to 15 skills
                    break

        return cleaned_skills

    def _extract_syllabus(self, soup: BeautifulSoup) -> List[Dict]:
        """Extract course syllabus/modules"""
        syllabus = []
        module_tags = soup.find_all(['div', 'section'], class_=['module', 'Module'])

        for idx, module in enumerate(module_tags, 1):
            module_data = {
                'week': idx,
                'title': module.get_text(strip=True)[:100],
                'topics': []
            }

            topic_tags = module.find_all(['li', 'span'])
            module_data['topics'] = [t.get_text(strip=True) for t in topic_tags[:5]]

            syllabus.append(module_data)

        return syllabus

    def _extract_prerequisites(self, soup: BeautifulSoup) -> List[str]:
        """Extract course prerequisites"""
        prereq_section = soup.find(['div', 'section'], class_=['prerequisite', 'Prerequisite'])

        if prereq_section:
            prereq_items = prereq_section.find_all(['li', 'p'])
            return [item.get_text(strip=True) for item in prereq_items]

        return []

    def _extract_price(self, soup: BeautifulSoup) -> str:
        """Extract course price"""
        price_tag = soup.find(['span', 'div'], class_=['price', 'Price'])
        if price_tag:
            return price_tag.get_text(strip=True)

        # Check for "Free" text
        free_tag = soup.find(string=lambda text: text and 'free' in text.lower())
        return "Free" if free_tag else "N/A"

    def _extract_certificate(self, soup: BeautifulSoup) -> bool:
        """Check if certificate is offered"""
        cert_tag = soup.find(string=lambda text: text and 'certificate' in text.lower())
        return cert_tag is not None

    def crawl_courses(self, search_queries: List[str], max_per_query: int = 10, delay: int = 2) -> List[Dict]:
        """
        Crawl courses for multiple search queries

        Args:
            search_queries: List of search terms
            max_per_query: Maximum courses to scrape per query
            delay: Delay in seconds between requests (to be respectful)

        Returns:
            List of course data dictionaries
        """
        all_courses = []

        for query in search_queries:
            logger.info(f"Processing query: {query}")

            # Get course URLs
            course_urls = self.search_courses(query, max_results=max_per_query)

            # Extract data from each course
            for url in course_urls:
                course_data = self.extract_course_info(url)
                if course_data:
                    all_courses.append(course_data)

                # Be respectful - add delay between requests
                time.sleep(delay)

            logger.info(f"Completed query: {query} - Total courses: {len(all_courses)}")

        return all_courses

    def save_to_json(self, data: List[Dict], filename: str = 'coursera_courses.json'):
        """
        Save scraped data to JSON file

        Args:
            data: List of course data dictionaries
            filename: Output filename
        """
        try:
            with open(filename, 'w', encoding='utf-8') as f:
                json.dump(data, f, indent=2, ensure_ascii=False)

            logger.info(f"Data saved to {filename} - Total courses: {len(data)}")

        except Exception as e:
            logger.error(f"Error saving data to JSON: {e}")


def crawl_sfia_skills(sfia_skills_data: Dict, max_per_skill: int = 3, delay: int = 2) -> Dict:
    """
    Crawl Coursera courses for SFIA skills

    Args:
        sfia_skills_data: Dictionary containing SFIA skills data
        max_per_skill: Maximum courses to scrape per skill
        delay: Delay in seconds between requests

    Returns:
        Dictionary mapping skills to their courses
    """
    scraper = CourseraScraper()
    results = {
        "scraping_metadata": {
            "scraped_at": datetime.now().isoformat(),
            "total_skills": len(sfia_skills_data.get("skills", [])),
            "max_courses_per_skill": max_per_skill
        },
        "skills_with_courses": []
    }

    skills = sfia_skills_data.get("skills", [])

    for idx, skill in enumerate(skills, 1):
        skill_name = skill.get("skill_name", "")
        skill_code = skill.get("skill_code", "")
        skill_id = skill.get("skill_id", "")

        logger.info(f"Processing skill {idx}/{len(skills)}: {skill_name} ({skill_code})")

        # Search for courses related to this skill
        course_urls = scraper.search_courses(skill_name, max_results=max_per_skill)

        courses = []
        for url in course_urls:
            course_data = scraper.extract_course_info(url)
            if course_data:
                courses.append(course_data)
            time.sleep(delay)

        skill_result = {
            "skill_id": skill_id,
            "skill_name": skill_name,
            "skill_code": skill_code,
            "level_count": skill.get("level_count", 0),
            "courses_found": len(courses),
            "courses": courses
        }

        results["skills_with_courses"].append(skill_result)
        logger.info(f"Found {len(courses)} courses for {skill_name}")

    return results


def main():
    """Main execution function - Searches Coursera for all SFIA skills"""

    # SFIA Skills data
    sfia_skills_data = {
        "success": True,
        "skills": [
            {"skill_id": "30000000-0000-0000-0000-000000000078", "skill_name": "Accessibility and inclusion", "skill_code": "ACIN", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000117", "skill_name": "Analytical classification and coding", "skill_code": "ANCC", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000066", "skill_name": "Animation development", "skill_code": "ADEV", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000090", "skill_name": "Application support", "skill_code": "ASUP", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000025", "skill_name": "Artificial intelligence (AI) and data ethics", "skill_code": "ATIC", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000107", "skill_name": "Asset management", "skill_code": "APTS", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000026", "skill_name": "Audit", "skill_code": "AUDT", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000102", "skill_name": "Availability management", "skill_code": "AVMT", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000012", "skill_name": "Benefits management", "skill_code": "BENM", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000138", "skill_name": "Bid/proposal management", "skill_code": "BIDM", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000143", "skill_name": "Brand management", "skill_code": "BRMG", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000013", "skill_name": "Budgeting and forecasting", "skill_code": "BUDF", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000137", "skill_name": "Business administration", "skill_code": "ADMN", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000073", "skill_name": "Business intelligence", "skill_code": "BINT", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000040", "skill_name": "Business modelling", "skill_code": "BSMO", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000042", "skill_name": "Business process improvement", "skill_code": "BPRE", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000037", "skill_name": "Business situation analysis", "skill_code": "BUSA", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000103", "skill_name": "Capacity management", "skill_code": "CPMG", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000129", "skill_name": "Certification scheme operation", "skill_code": "CSOP", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000106", "skill_name": "Change control", "skill_code": "CHMG", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000128", "skill_name": "Competency assessment", "skill_code": "LEDA", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000095", "skill_name": "Configuration management", "skill_code": "CFMG", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000029", "skill_name": "Consultancy", "skill_code": "CNSL", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000082", "skill_name": "Content design and authoring", "skill_code": "INCA", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000083", "skill_name": "Content publishing", "skill_code": "COPU", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000134", "skill_name": "Contract management", "skill_code": "ITCM", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000015", "skill_name": "Cost management", "skill_code": "COAG", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000144", "skill_name": "Customer engagement and loyalty", "skill_code": "CTLO", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000077", "skill_name": "Customer experience", "skill_code": "CEXP", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000136", "skill_name": "Customer service support", "skill_code": "CSMG", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000113", "skill_name": "Cybercrime investigation", "skill_code": "CRIM", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000070", "skill_name": "Data analytics", "skill_code": "DATS", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000074", "skill_name": "Data engineering", "skill_code": "DENG", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000067", "skill_name": "Data management", "skill_code": "DATM", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000068", "skill_name": "Data modelling and design", "skill_code": "DTAN", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000071", "skill_name": "Data science", "skill_code": "DASC", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000075", "skill_name": "Data visualisation", "skill_code": "VISL", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000118", "skill_name": "Database administration", "skill_code": "DBAD", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000069", "skill_name": "Database design", "skill_code": "DBDS", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000036", "skill_name": "Delivery management", "skill_code": "DEMG", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000016", "skill_name": "Demand management", "skill_code": "DEMM", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000097", "skill_name": "Deployment", "skill_code": "DPLS", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000112", "skill_name": "Digital forensics", "skill_code": "DGFS", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000146", "skill_name": "Digital marketing", "skill_code": "DIGM", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000007", "skill_name": "Emerging technology monitoring", "skill_code": "EMRG", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000120", "skill_name": "Employee experience", "skill_code": "EEXP", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000004", "skill_name": "Enterprise and business architecture", "skill_code": "STPL", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000099", "skill_name": "Facilities management", "skill_code": "DCMA", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000038", "skill_name": "Feasibility assessment", "skill_code": "FEAS", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000014", "skill_name": "Financial analysis", "skill_code": "FNAN", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000010", "skill_name": "Financial management", "skill_code": "FMIT", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000008", "skill_name": "Formal research", "skill_code": "RSCH", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000023", "skill_name": "Governance", "skill_code": "GOVN", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000085", "skill_name": "Graphic design", "skill_code": "GRDN", "level_count": 6},
            {"skill_id": "30000000-0000-0000-0000-000000000055", "skill_name": "Hardware design", "skill_code": "HWDE", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000088", "skill_name": "High-performance computing", "skill_code": "HPCC", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000110", "skill_name": "Identity and access management", "skill_code": "IAMT", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000104", "skill_name": "Incident management", "skill_code": "ICPM", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000020", "skill_name": "Information and data compliance", "skill_code": "PEDP", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000019", "skill_name": "Information assurance", "skill_code": "INAS", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000003", "skill_name": "Information management", "skill_code": "IRMG", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000018", "skill_name": "Information security", "skill_code": "SCTY", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000002", "skill_name": "Information systems coordination", "skill_code": "ISCO", "level_count": 2},
            {"skill_id": "30000000-0000-0000-0000-000000000054", "skill_name": "Infrastructure design", "skill_code": "IFDN", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000091", "skill_name": "Infrastructure operations", "skill_code": "ITOP", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000006", "skill_name": "Innovation management", "skill_code": "INOV", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000011", "skill_name": "Investment appraisal", "skill_code": "INVA", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000044", "skill_name": "Job analysis and design", "skill_code": "JAAN", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000084", "skill_name": "Knowledge management", "skill_code": "KNOW", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000125", "skill_name": "Learning and development management", "skill_code": "ETMG", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000127", "skill_name": "Learning delivery", "skill_code": "ETDL", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000126", "skill_name": "Learning design and development", "skill_code": "TMCH", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000072", "skill_name": "Machine learning", "skill_code": "MLNG", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000142", "skill_name": "Market research", "skill_code": "MRCH", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000145", "skill_name": "Marketing campaign management", "skill_code": "MECM", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000141", "skill_name": "Marketing management", "skill_code": "MKMG", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000017", "skill_name": "Measurement", "skill_code": "MEAS", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000031", "skill_name": "Methods and tools", "skill_code": "METL", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000053", "skill_name": "Network design", "skill_code": "NTDS", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000093", "skill_name": "Network support", "skill_code": "NTAS", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000059", "skill_name": "Non-functional testing", "skill_code": "NFTS", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000087", "skill_name": "Numerical analysis", "skill_code": "NUAN", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000114", "skill_name": "Offensive cyber operations", "skill_code": "OCOP", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000045", "skill_name": "Organisation design and implementation", "skill_code": "ORDI", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000047", "skill_name": "Organisational change enablement", "skill_code": "OCEN", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000046", "skill_name": "Organisational change management", "skill_code": "CIPM", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000121", "skill_name": "Organisational facilitation", "skill_code": "OFCL", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000043", "skill_name": "Organizational capability development", "skill_code": "OCDY", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000115", "skill_name": "Penetration testing", "skill_code": "PENT", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000119", "skill_name": "Performance management", "skill_code": "PEMT", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000032", "skill_name": "Portfolio management", "skill_code": "PFMG", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000035", "skill_name": "Portfolio, programme and project support", "skill_code": "PROF", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000105", "skill_name": "Problem management", "skill_code": "PBMG", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000060", "skill_name": "Process testing", "skill_code": "PRTS", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000048", "skill_name": "Product management", "skill_code": "PROD", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000122", "skill_name": "Professional development", "skill_code": "PDSV", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000033", "skill_name": "Programme management", "skill_code": "PGMG", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000056", "skill_name": "Programming/software development", "skill_code": "PROG", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000034", "skill_name": "Project management", "skill_code": "PRMG", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000028", "skill_name": "Quality assurance", "skill_code": "QUAS", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000027", "skill_name": "Quality management", "skill_code": "QUMG", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000065", "skill_name": "Radio frequency engineering", "skill_code": "RFEN", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000062", "skill_name": "Real-time/embedded systems development", "skill_code": "RESD", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000116", "skill_name": "Records management", "skill_code": "RMGT", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000096", "skill_name": "Release management", "skill_code": "RELM", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000039", "skill_name": "Requirements definition and management", "skill_code": "REQM", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000124", "skill_name": "Resourcing", "skill_code": "RESC", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000024", "skill_name": "Risk management", "skill_code": "BURM", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000064", "skill_name": "Safety assessment", "skill_code": "SFAS", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000063", "skill_name": "Safety engineering", "skill_code": "SFEN", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000140", "skill_name": "Sales support", "skill_code": "SSUP", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000086", "skill_name": "Scientific modelling", "skill_code": "SCMO", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000109", "skill_name": "Security operations", "skill_code": "SCAD", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000139", "skill_name": "Selling", "skill_code": "SALE", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000108", "skill_name": "Service acceptance", "skill_code": "SEAC", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000101", "skill_name": "Service catalogue management", "skill_code": "SCMG", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000100", "skill_name": "Service level management", "skill_code": "SLMO", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000061", "skill_name": "Software configuration", "skill_code": "PORT", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000052", "skill_name": "Software design", "skill_code": "SWDN", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000005", "skill_name": "Solution architecture", "skill_code": "ARCH", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000132", "skill_name": "Sourcing", "skill_code": "SORC", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000030", "skill_name": "Specialist advice", "skill_code": "TECH", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000135", "skill_name": "Stakeholder relationship management", "skill_code": "RLMT", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000098", "skill_name": "Storage management", "skill_code": "SRMG", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000001", "skill_name": "Strategic planning", "skill_code": "ITSP", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000131", "skill_name": "Subject formation", "skill_code": "SUBF", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000133", "skill_name": "Supplier management", "skill_code": "SUPP", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000009", "skill_name": "Sustainability", "skill_code": "SUST", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000092", "skill_name": "System software administration", "skill_code": "SYSP", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000050", "skill_name": "Systems and software lifecycle engineering", "skill_code": "SLEN", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000051", "skill_name": "Systems design", "skill_code": "DESN", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000049", "skill_name": "Systems development management", "skill_code": "DLMG", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000094", "skill_name": "Systems installation and removal", "skill_code": "HSIN", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000057", "skill_name": "Systems integration and build", "skill_code": "SINT", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000130", "skill_name": "Teaching", "skill_code": "TEAC", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000089", "skill_name": "Technology service management", "skill_code": "ITMG", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000058", "skill_name": "Testing", "skill_code": "TEST", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000022", "skill_name": "Threat intelligence", "skill_code": "THIN", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000041", "skill_name": "User acceptance testing", "skill_code": "BPTS", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000079", "skill_name": "User experience analysis", "skill_code": "UNAN", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000080", "skill_name": "User experience design", "skill_code": "HCEV", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000081", "skill_name": "User experience evaluation", "skill_code": "USEV", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000076", "skill_name": "User research", "skill_code": "URCH", "level_count": 5},
            {"skill_id": "30000000-0000-0000-0000-000000000111", "skill_name": "Vulnerability assessment", "skill_code": "VUAS", "level_count": 4},
            {"skill_id": "30000000-0000-0000-0000-000000000021", "skill_name": "Vulnerability research", "skill_code": "VURE", "level_count": 3},
            {"skill_id": "30000000-0000-0000-0000-000000000123", "skill_name": "Workforce planning", "skill_code": "WFPL", "level_count": 3}
        ],
        "total": 146
    }

    logger.info("Starting SFIA Skills Coursera scraper...")
    logger.info(f"Total skills to process: {len(sfia_skills_data['skills'])}")

    # Crawl courses for all SFIA skills
    results = crawl_sfia_skills(
        sfia_skills_data=sfia_skills_data,
        max_per_skill=3,  # Get up to 3 courses per skill
        delay=2  # 2 seconds between requests to be respectful
    )

    # Save to JSON
    if results and results.get("skills_with_courses"):
        output_filename = 'sfia_skills_coursera_courses.json'
        try:
            with open(output_filename, 'w', encoding='utf-8') as f:
                json.dump(results, f, indent=2, ensure_ascii=False)

            total_courses = sum(skill['courses_found'] for skill in results['skills_with_courses'])
            logger.info(f"Scraping completed!")
            logger.info(f"Total skills processed: {len(results['skills_with_courses'])}")
            logger.info(f"Total courses collected: {total_courses}")
            logger.info(f"Data saved to {output_filename}")
        except Exception as e:
            logger.error(f"Error saving data to JSON: {e}")
    else:
        logger.warning("No courses were collected")


if __name__ == "__main__":
    main()
