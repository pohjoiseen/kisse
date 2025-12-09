Kisse
=====

This is **Kisse**, a very simple web application to track encounters of outdoors cats in the local area.
It is written for personal use, as we in our family are very much cat people and like to tell each other about
cats seen on walks in our beautiful Finnish suburbia/countryside mix.  It is deployed to a private server.

It was also meant to be a small exercise in a new (for me) frontend technology.  The app is written in
C#, in ASP.NET Core MVC 10, and the frontend framework is htmx, which means it is server-rendered, as in good old
days, but still nicely responsive.  Map functionality uses the traditional Leaflet library
(OSM for actual map tiles), and fitting Leaflet into an otherwise htmx-powered app adds a bit to the challenge.
For database (which only has three tables, not counting ASP.NET Identity stuff) I used sqlite.  Photos and
their thumbnails are uploaded just to local filesystem.  We extract photo dates and GPS coordinates from EXIF
metadata so that everything is automatically placed on the map wherever possible.

Everything should be very straightforward, all business logic is in controllers and UI logic in Razor templates;
the project is too small to bother with a service layer.  There are only a few JS files, and all libraries are
loaded from jsdelivr.  Thus, no frontend build system and no need for one.  Bootstrap is used as frontend library,
very lightly customized.  CSS view transitions are very helpful here to make the server-rendered app more responsive.
The app is meant primarily for mobile use (and can and should be installed as a PWA), but of course works
on desktop too; especially browsing cat/observations lists is for sure nicer on the desktop.

The app is not meant to display huge amount of data (how many different cats are we likely ever to see, hundreds
at most?) and in particular in the main map view everything is loaded at once.  Of course this can be improved,
if it ever somehow becomes an actual problem.

TODO
====

* Removery jQuery/Bootstrap JS dependence (only ever used for navbar expansion)
* Dockerize?
* Handle CSRF in forms probably
* Is there any danger of "database is locked" errors in sqlite?

Alexander Ulyanov