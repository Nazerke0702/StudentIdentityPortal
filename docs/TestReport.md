# Тестілеу есебі

**Жоба:** StudentIdentityPortal  
**Орындаушы:** Nazerke  
**Тексеру күні:** 2026-11-27  
**Ортасы:** .NET 10, xUnit, ASP.NET Core WebApplicationFactory, in-memory SQLite  
**Деректер қауіпсіздігі:** интеграциялық тесттер жеке жадтағы SQLite дерекқорын қолданды. Жобадағы development дерекқоры өзгертілмеді.

## Қорытынды

dotnet test .\StudentIdentityPortal.slnx --no-restore --maxcpucount:1 -v:minimal пәрмені **26 тесті сәтті аяқтады: 26 PASS, 0 FAIL, 0 SKIP**. Тесттер нақты MVC endpoint-теріне HTTP сұраныстарын жіберіп, cookie, antiforgery және рөлдік авторизацияны тексереді.

Қосымша Development Kestrel smoke-тесті жеке уақытша SQLite файлымен орындалды: Home және Register 200, anonymous Dashboard Login-ге бағыттады, User Secrets-ке ұқсас орта айнымалыларымен seed болған Admin жүйеге кіріп Admin бетін аша алды. Жобаның қолданыстағы дерекқоры қолданылған жоқ.

Браузер скриншоттарының ішінен 01-home.png ғана бар. Қалған он скриншот осы ортада npm registry қолжетімсіз болып, Playwright Chromium орнатылмағандықтан түсірілмеді. Оларды қауіпсіз бөлек SQLite дерекқорын қолданатын scripts/Capture-Evidence.ps1 Playwright сценарийі түсіруге дайын. Түсірілмеген суреттер PASS деп белгіленбейді.

## Сценарийлер

| Test ID | Атауы және рөлі | Қадамдар | Күтілетін нәтиже | Нақты нәтиже | Status | Скриншот дәлелі |
| --- | --- | --- | --- | --- | --- | --- |
| TEST-01 | Қонақ ашық беттерді ашады — Guest | /, /Account/Register, /Account/Login беттеріне GET | Барлық бет 200 қайтарады | 200; xUnit интеграциялық тесті өтті | PASS | [01-home.png](screenshots/01-home.png) |
| TEST-02 | Қонақ қорғалған бетке кіреді — Guest | /Dashboard адресіне GET | Login бетіне redirect | /Account/Login адресіне redirect; тест өтті | PASS | Сурет түсірілмеді |
| TEST-03 | Дұрыс деректермен тіркелу — Guest | CSRF token алып, толық форма жіберу | Student жасалып, Dashboard ашылады | Redirect және аутентификацияланған Dashboard; тест өтті | PASS | Түсірілмеді: [Playwright сценарийі](../scripts/capture-evidence.mjs) |
| TEST-04 | Қате тіркелу деректері — Guest | Бос аттар, қате email, әлсіз/сәйкес емес пароль жіберу | Пайдаланушы жасалмай, validation қатесі шығады | 200 форма жауабы, field errors бар; тест өтті | PASS | Түсірілмеді: Playwright сценарийі |
| TEST-05 | Жаңа аккаунттың рөлін тексеру — Student | Тіркелген email арқылы UserManager-ден рөлдерді оқу | Тек Student рөлі беріледі | Рөлдер тізімі тек Student; тест өтті | PASS | Түсірілмеді |
| TEST-06 | Жалған Admin рөлін жіберу — Guest | Формаға қосымша Role=Admin мәнін қосып тіркелу | Клиент жіберген рөл еленбейді | Аккаунт Student, Admin емес; тест өтті | PASS | Түсірілмеді |
| TEST-07 | Қайталанатын email — Guest | Бір email-мен екінші рет тіркелуге әрекет жасау | Екінші аккаунт құрылмайды; қате көрсетіледі | Пайдаланушы саны өзгермеді; тест өтті | PASS | Түсірілмеді |
| TEST-08 | Қате пароль және белгісіз аккаунт — Guest | Белгілі email-ге қате пароль, кейін жоқ email жіберу | Екі жағдайда да кіру қабылданбайды, ортақ қате беріледі | Екі сұраныс та 200 қате формасын қайтарды; тест өтті | PASS | Түсірілмеді: Playwright 06-login-validation.png түсіреді |
| TEST-09 | Student кіруі және рұқсат етілген беттері — Student | Тіркелу, logout, қайта login; Dashboard, Students, Profile ашу | Рұқсат етілген беттер 200 қайтарады | Үшеуі де 200; тест өтті | PASS | Түсірілмеді: Playwright 07-student-dashboard.png түсіреді |
| TEST-10 | Student Admin URL-ін тікелей ашады — Student | /Admin адресіне GET | Access Denied бетіне redirect | /Account/AccessDenied адресіне redirect; тест өтті | PASS | Түсірілмеді: Playwright 08-student-access-denied.png түсіреді |
| TEST-11 | Student Admin әрекетін тікелей жібереді — Student | Дұрыс CSRF token-мен /Admin/DeleteStudent POST жасау | Рөлдік тексеру әрекетті бұғаттайды | Access Denied redirect; тест өтті | PASS | Түсірілмеді: Playwright 08-student-access-denied.png түсіреді |
| TEST-12 | Admin панеліне кіру — Admin | Тест ортасында Admin жасап, login болып /Admin ашу | Admin Dashboard 200 қайтарады | Dashboard ашылды; тест өтті | PASS | Түсірілмеді: Playwright 09-admin-dashboard.png түсіреді |
| TEST-13 | Рұқсат етілген Admin әрекеті — Admin | CSRF token арқылы Student-ті жою | Тек Student өшіріледі, Admin бетіне қайтады | 302 /Admin; студент табылмады; тест өтті | PASS | Түсірілмеді: Playwright 10-admin-protected-action.png түсіреді |
| TEST-14 | GET арқылы Logout жасау — Student | /Account/Logout адресіне GET, артынан Dashboard ашу | GET logout жасамайды | GET 404/405; Dashboard 200; тест өтті | PASS | Түсірілмеді |
| TEST-15 | CSRF token-мен POST Logout — Student | Dashboard token-імен logout POST; кейін Dashboard ашу | Сессия жабылып, қорғалған бет Login талап етеді | Home redirect; Dashboard Login-ге redirect; тест өтті | PASS | Түсірілмеді: Playwright 11-logout.png түсіреді |
| TEST-16 | Token жоқ Logout — Student | Logout endpoint-іне token-сіз POST | Сұраныс қабылданбайды, сессия сақталады | 400; Dashboard 200; тест өтті | PASS | Түсірілмеді |
| TEST-17 | Open Redirect тексеруі — Student | https://evil.example ReturnUrl арқылы дұрыс login | Сыртқы адреске redirect болмайды | /Dashboard адресіне redirect; тест өтті | PASS | Түсірілмеді |
| TEST-18 | CSRF token жоқ тіркелу — Guest | Token-сіз Register POST жіберу | Сұраныс бұғатталады | 400 Bad Request; тест өтті | PASS | Түсірілмеді |
| TEST-19 | Testing ортод Admin auto-seed — Testing | Admin конфигурациясы бар Testing host іске қосу | Admin аккаунты автоматты құрылмайды | Admin табылмады; тест өтті | PASS | Скриншот қарастырылмайды |
| TEST-20 | ViewModel дұрыс деректері — Unit | Дұрыс аты, email және парольді тексеру | Validation қатесі жоқ | Қате жоқ; тест өтті | PASS | Скриншот қарастырылмайды |
| TEST-21 | Әлсіз парольдер — Unit | Кіші әріп, бас әріп, цифр, символ немесе ұзындық талабы жетіспейтін бес пароль | Бес жағдай да қабылданбайды | 5/5 data row өтті; тест өтті | PASS | Скриншот қарастырылмайды |
| TEST-22 | ConfirmPassword сәйкес емес — Unit | Әртүрлі пароль мен растауды тексеру | Сәйкессіздік қатесі шығады | ConfirmPassword қатесі бар; тест өтті | PASS | Скриншот қарастырылмайды |
| TEST-23 | Development Kestrel smoke — Guest және Admin | Оқшауланған уақытша DB-пен сервер іске қосу; public, protected және Admin маршруттарын тексеру | Home/Register ашылады; қонақ login-ге бағытталады; Development Admin кіріп басқару бетін ашады | Home 200, Register 200, Dashboard → Login, Admin login → Dashboard, /Admin 200 | PASS | Сурет түсірілмеді |

## Скриншот күйі

| Файл | Күйі | Ескерту |
| --- | --- | --- |
| [01-home.png](screenshots/01-home.png) | PASS | Іске қосылған қосымшаның нақты home бетінен алынған |
| 02-register.png | NOT RUN | Playwright арқылы түсіру керек |
| 03-register-validation.png | NOT RUN | Playwright арқылы түсіру керек |
| 04-register-success.png | NOT RUN | Playwright арқылы түсіру керек |
| 05-login.png | NOT RUN | Playwright арқылы түсіру керек |
| 06-login-validation.png | NOT RUN | Playwright арқылы түсіру керек |
| 07-student-dashboard.png | NOT RUN | Playwright арқылы түсіру керек |
| 08-student-access-denied.png | NOT RUN | Playwright арқылы түсіру керек |
| 09-admin-dashboard.png | NOT RUN | Playwright арқылы түсіру керек |
| 10-admin-protected-action.png | NOT RUN | Playwright арқылы түсіру керек |
| 11-logout.png | NOT RUN | Playwright арқылы түсіру керек |

## Қорғаныс тексерулері

- Тіркелу кезінде рөл серверде Student болып белгіленеді; жалған Role өрісі әсер етпейді.
- Admin controller-і мен әрекеттері рөлмен қорғалған; Student тікелей URL және POST арқылы өте алмайды.
- Logout тек POST; GET сессияны жаппайды. Antiforgery token жоқ POST 400 қайтарады.
- Қорғалған бет logout-тен кейін Login талап етеді.
- ReturnUrl сыртқы сайтқа бағыттамайды.
- Admin пайдаланушысы автоматты түрде тек Development ортасында, User Secrets немесе environment settings бапталған кезде ғана seed етіледі. Тест DB жадта уақытша сақталады.
