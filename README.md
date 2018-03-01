# Exallon
Exallon позволяет просматривать информацию в программе 1С:Предприятие (далее 1С). Текущая версия программы работает со справочниками 1С в режиме просмотра.

### Начало работы
Перед началом работы с базами 1С необходимо произвести настройки доступа. 

*Настройка подключений к базам данных*
Необходимо указать адрес сервера Exallon, имя пользователя в системе 1С и, при необходимости, пароль.
Адрес сервера имеет формат:
```
<url-сервера>[:<порт>]/<путь к БД>
```
Например: demo.exallon.ru:8080/test

*Сохранить пароль*
Используется для сохранения пароля доступа к базе 1С, чтобы при последующих запусках программы не вводить его заново.

*Вход в приложение*
Для входа необходимо выбрать одно из ранее настроенных подключений к базе данных и ввести регистрационные данные пользователя в соответствующие поля. В случае использование атоматического входа при подключении имя пользователя и пароль запрашиваться не будут.
> Если вам не удалось выполнить вход в программу, проверьте корректность ввода всех учетных данных. Для этого нажмите и удерживайте требуемый элемент в списке баз данных.

*Переключение между базами данных и выход*
После выполнения входа в Exallon, нажмите Menu на вашем android-устройстве и выберите Завершить сеанс. В этом случае будет произведено отключение от базы 1С и переход к окну выбора баз данных.

### Работа со справочниками
Нажмите на Справочники в верхней части экрана для просмотра списка справочников. В списке будут представлены только те справочники, которые разрешены для удаленной работы в Exallon. Нажмите на интересующий вас справочник для его просмотра.  При просмотре, сначала отображаются группы справочника, а затем его элементы. Для просмотра интересующего элемента или группы, просто выберите его.

> Все настройки доступа к справочникам и настройка  реквизитов производятся на сервере Exallon. 

### Работа с фильтром
Фильтр позволяет быстро найти необходимый справочник или элемент справочника. При вводе в строку поиска текста происходит фильтрация по указанному значению. Все найденные наименования выводятся в списке. Поиск регистронезависимый. Фильтр применяется только к текущему списку. Если загружен список справочников, то фильтр работает с этим списком. Фильтр по справочнику работает со всем справочником, включая все группы.

### Избранное
Для перехода в Избранное нажмите на соответствующую иконку в верхней части экрана. Избранное позволяет сохранить ссылки на наиболее часто используемые элементы базы 1С. Это может быть какой-либо справочник, группа или конкретный элемент справочника. Для добавления в избранное нажмите и удерживайте интересующий элемент. Для удаления из избранного необходимо нажать и удерживать требуемый элемент в окне Избранное.