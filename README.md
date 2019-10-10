# Spotify-Muter
I do not like Spotify ads, so I created an app that mutes Spotify any time an ad pops up, and un-mutes when the ads are finished.

## About This Project
This program is actually very simple. The program continually reads the title of the Spotify window, which is changed based on what song is playing.
If it is an actual song, the format will be "ARTIST - SONGNAME". However, if an ad is currently playing, the format will most likely not match. 
Every so often, an ad will pop up with the exact format, but can be added to an artist blacklist by pushing the simple button on the GUI.

## Things I Learned During Development
Initially, I attempted to use the official Spotify API. Unfortunately, 
the C# scripts I tried using to access it were outdated and failed. This
forced me to find another approach, which turned out to be a lot simpler.
In addition, I had to learn how to access Window's volume control in order
to actually mute the Spotify window. 
