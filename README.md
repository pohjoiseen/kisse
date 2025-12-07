Kisse
=====

This is **Kisse**, a very simple web application to track encounters of outdoors cats in the local area.
It is written for personal use, as we in our family are very much cat people and like to tell each other about
cats seen on walks in our beautiful Finnish suburbia/countryside mix.  It is deployed to a private server.

It was also meant to be a small exercise in a new (for me) frontend technology.  The app is written in
C#, in ASP.NET Core MVC 10, and the frontend framework is htmx, which means it is server-rendered, as in good old
days, but still nicely responsive.  Map functionality uses the traditional Leaflet library
(OSM for actual map tiles), and fitting Leaflet into an otherwise htmx-powered app adds a bit to the challenge.
For database (which only has three tables, not counting ASP.NET Identity stuff) I used sqlite.


TODO
====

* Customize appearance a bit from default Bootstrap
* Dockerize?
* Deploy
* Handle CSRF in forms probably
* Is there any danger of "database is locked" errors in sqlite?

Alexander Ulyanov