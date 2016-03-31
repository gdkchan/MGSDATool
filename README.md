# MGSDATool
Metal Gear Solid 3/4 translation toolkit

This is a toolkit for translating Metal Gear Solid 3: Subsistence and Metal Gear Solid 4: Guns of the Patriots.

Below you can see a list of what each tool does:
DATCodecTool - Extracts texts from the codec.dat file and inserts then back. Works on both MGS3 and MGS4.
DATMovieTool - Extracts subtitles from movie.dat and demo.dat, and can also insert the modified subtitles back. Like the codec tool, works on both MGS3 and MGS4.
DATSpeechTool - Extracts *.spc files from a speech.dat (with a scenerio.gcx from the init folder), and extracts subtitles from each *.spc. It can insert subtitles back into the *.spc, and can re-create the speech.dat aswell. Only works on MGS4, since MGS3 don't have a speech.dat file.

A note about the DATCodecTool and DATMovieTool: It's still unknown where the pointers inside those files (codec.dat, movie.dat and demo.dat) are, therefore if you change the lengths of the texts inside those files, there's a possibility that the game will crash. Due to the fact that those files have paddings (with movie.dat and demo.dat being aligned into 2kb blocks), there's usually a lot of room for expansion. But again, it's recommended that you keep the same lengths, or recalculate the pointers if you know where they are.
Please look carefully the usage instructions of each tool, since each tool is used in a different way.