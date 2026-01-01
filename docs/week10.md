### spatial frequency domain
#### Spatial frequency domain

#### 1D
frequency : cycles in a certain mount of time

amplitude : range of high and low values

phase : where we are in the cycle in the sin wave

frequency : one quickly going up and one going down

amplitude : range of amplitude in frequency domain (spectrum)

phase : do nothing?

#### Decomposition and synthesis

Time domain : amplitude & time
Frequency domain : amplitude & frequency

#### 1D to 2D sine waves
spatial frequency: same
amplitude: same(color of the waves)
phase: same
orientation: spin images

##### spectrum
DC component : centre of 2D spectrum, represents arithmetic mean of all pixel values.
Lower spatial frequencies appear as component near to the centre of 2D spectrum.
Value of the components correspond to the amplitudes.
to make the easy to the interupt, 2d spectrum are reflect cross the image
Orientation : rotate image will rotate the pixels position correspond to the DC component.
Phase : do nothing
strike energy

#### 2D sinewave spectra (main application : compression)
2 ways of compress:
input -> SD filter -> output
input -> FFT -> SFD filter -> iFFT -> output

#### Spatial domain

#### filters in SFD
SFPD lowpass-filter: frequency domain pass dis should be tapered off at the borders ( switch off some frequencies ) (isomg 2D gaussian or butter worth function)

band pass : keep things in a set of frequency

high pass : keep high frequency

##### SFD Orientation filters
Selective : only keep a range of orientation, can be hybrid with frequency filters.


#### examples SFD application
- Orientation of image
- eliminate periodic noises (frequency domain)

JPEG 
- each 8x8 block of pixels is subject to a spatial frequency tranform (DCT)
- compression : coarsely representing high frequencies (remove frequency is the button-right corner).


