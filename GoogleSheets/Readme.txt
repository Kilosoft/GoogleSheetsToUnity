Документация по таблицам:
https://developers.google.com/sheets/api/quickstart/python

Документация по PyLinq:
https://viralogic.github.io/py-enumerable/


1. Если не заработало надо установить питон выше 2,7
2. Установить модули
в командной строке вводим

pip install pip
pip install --upgrade google-api-python-client google-auth-httplib2 google-auth-oauthlib
pip install py-linq

3. Возможно потребуется пересоздать файл credentials.json, нужно если не авторизовывается. надо:
    - Зайти по ссылке документации
    - Перейти к пункту Step1
    - Нажать кнопку Enable the Google Sheets API
    
4. Запустите скрипт например items.py
5. Рядом появится файл items.json