# Webserver

Extends a simple line-echoing network server into a bare-bones HTTP/1.0 server named webserver. To keep things manageable, the server:

Supports only “GET” requests (without querystrings or other “extra” fields), returning only a subset of the standard HTTP error codes: 

* 200 (success)
* 400 (malformed request)
* 403 (requested a file outside the “document root”)
* 404 (requested a non-existent file)
* 501 (unsupported request “verb” [e.g., “POST”])
* 500 (any other error)

Ignores the contents of all request headers (but verifies that the request is in the proper format)

Therefore, does not support any HTTP features depending upon request headers (content encodings, keep-alive, etc.)

This summary describes the full implementation.

## Time Log for Melchisedek Dulcio | Total: 11 hrs

| Date | Accomplished | Time Spent: 42 hrs |
|---|---|---|
| 3/8/2020 | Setup for Project | 1 hr |
| 4/02/2020 | Incorporating parseHttp | 30 mins |
| 4/03/2020 | Finishing CheckPoints | 6 hrs |
| 4/06/2020 | Finishing 80 Percent | 2 hrs |
| 4/07/2020 | Error Handling and Testing | 4 hrs |
| 4/08/2020 | Bug fixing/ work on 90 Level | 5 hrs |
| 4/09/2020 | Testing 90 Level | 5 hrs |
| 4/10/2020 | IO Error Handling | 6 hrs |
| 4/10/2020 | Multitasking Implementing | 3 hrs |
| 4/11/2020 | Bug Fixing | 9 hrs |
