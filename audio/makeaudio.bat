@echo off
rem Install LMMS and FFMPEG before conversion.
rem These software can be installed via Chocolatey.

setlocal
set lmmspath=c:\program files\lmms
set outdir=out

mkdir %outdir%
for %%k in (*.mmpz) do (
  "%lmmspath%\lmms" -s 44100 -r "%%~nk.mmpz" -o "%%~nk.wav"
  ffmpeg -i "%%~nk.wav" -af silenceremove=0:0:0:-1:0:0 -ar 20500 -ac 1 -y "%outdir%\%%~nk.wav"
)
endlocal