# Demo app

Simply run the app and it will play a simple sine wave.

Or you can run the app with the following arguments:

```
Description:
  Plays a demo audio.

Usage:
  Demo [options]

Options:
  -f, --format <f32le|s16le|u8>  The format of the output file [default: f32le]
  -r, --rate <rate>              The sample rate [default: 44100]
  -c, --channels <channels>      The number of channels [default: 2]
  -w, --wave <sine|square>       The type of waive to generate [default: sine]
  -v, --vol <vol>                The volume level [default: 50]
  --version                      Show version information
  -?, -h, --help                 Show help and usage information
```

Example:

```shell
./Demo -w square -f s16le -r 11025 -v 50 -c 1

./Demo --wave square --format u8 --rate 44100

./Demo --wave square --format f32le --rate 44100
```
