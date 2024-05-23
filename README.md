# rlisten

This console program runs and **listens**/**polls** for changes in the most **frequently posting user**/**post with highest upvotes** on selected subreddits.

To run this program you need a ClientId and ClientSecret from the reddit API.

You can provide them as respective arguments on the command line or edit appsettings.json
where the following stanza exists:

```json
  "RedditOAuth": {
    "ClientId": "",
    "ClientSecret": ""
  }
```
Once connected to reddit, you are prompted to enter a subreddit name to monitor.  Then you can add or remove more subreddits using keystrokes at the console.  Press X to exit (when not pending for other input).

Here are some stats regarding this project.
```
      70 text files.
      52 unique files.
     195 files ignored.

github.com/AlDanial/cloc v 1.94  T=0.04 s (1223.8 files/s, 226076.7 lines/s)
------------------------------------------------------------------------------------
Language                          files          blank        comment           code
------------------------------------------------------------------------------------
JSON                                 12              0              0           7398
C#                                   26            292            108           1363
Text                                  2              0              0            195
XML                                   6              0              0            109
MSBuild script                        2              8              0             56
Visual Studio Solution                1              1              1             29
C# Generated                          2              8             18             19
Markdown                              1              0              0              1
------------------------------------------------------------------------------------
SUM:                                 52            309            127           9170
------------------------------------------------------------------------------------
```
