/*
 * Copyright (c) 2015 Allan Pichardo
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *  http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class AudioProcessor : MonoBehaviour
{
    public AudioSource audioSource;

    private long lastT, nowT, diff, entries, sum;

    public int bufferSize = 1024; // fft size
    private int samplingRate = 44100; // fft sampling frequency

    /* Log-frequency averaging controls */
    private int nBand = 12; // number of bands

    public float gThresh = 0.1f; // sensitivity

    int blipDelayLen = 16;
    int[] blipDelay;

    private int sinceLast = 0; // counter to suppress double-beats

    private float framePeriod;

    /* Storage space */
    private int colmax = 120;
    float[] spectrum;
    float[] averages;
    float[] acVals;
    float[] onsets;
    float[] scorefun;
    float[] dobeat;
    int now = 0; // time index for circular buffer within above

    float[] spec; // the spectrum of the previous step

    /* Autocorrelation structure */
    int maxlag = 100; // (in frames) largest lag to track
    float decay = 0.997f; // smoothing constant for running average
    Autoco auco;

    private float alph; // trade-off constant between tempo deviation penalty and onset strength

    [Header("Events")]
    public OnBeatEventHandler onBeat;
    public OnSpectrumEventHandler onSpectrum;

    //////////////////////////////////
    private long getCurrentTimeMillis()
    {
        long milliseconds = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
        return milliseconds;
    }

    private void initArrays()
    {
        blipDelay = new int[blipDelayLen];
        onsets = new float[colmax];
        scorefun = new float[colmax];
        dobeat = new float[colmax];
        spectrum = new float[bufferSize];
        averages = new float[12];
        acVals = new float[maxlag];
        alph = 100 * gThresh;
    }

    // Use this for initialization
    void Start()
    {
        initArrays();

        audioSource = GetComponent<AudioSource>();
        samplingRate = audioSource.clip.frequency;

        framePeriod = (float)bufferSize / (float)samplingRate;

        // Initialize record of previous spectrum
        spec = new float[nBand];
        for (int i = 0; i < nBand; ++i)
            spec[i] = 100.0f;

        auco = new Autoco(maxlag, decay, framePeriod, getBandWidth());

        lastT = getCurrentTimeMillis();
    }

    public void tapTempo()
    {
        nowT = getCurrentTimeMillis();
        diff = nowT - lastT;
        lastT = nowT;
        sum = sum + diff;
        entries++;

        int average = (int)(sum / entries);

        Debug.Log("average = " + average);
    }

    double[] toDoubleArray(float[] arr)
    {
        if (arr == null)
            return null;
        int n = arr.Length;
        double[] ret = new double[n];
        for (int i = 0; i < n; i++)
        {
            ret[i] = (float)arr[i];
        }
        return ret;
    }

    // Update is called once per frame
    void Update()
    {
        if (audioSource.isPlaying)
        {
            audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
            computeAverages(spectrum);
            onSpectrum.Invoke(averages);

            /* Calculate the value of the onset function in this frame */
            float onset = 0;
            for (int i = 0; i < nBand; i++)
            {
                float specVal = (float)System.Math.Max(-100.0f, 20.0f * (float)System.Math.Log10(averages[i]) + 160); // dB value of this band
                specVal *= 0.025f;
                float dbInc = specVal - spec[i]; // dB increment since last frame
                spec[i] = specVal; // record this frame to use next time around
                onset += dbInc; // onset function is the sum of dB increments
            }

            onsets[now] = onset;

            /* Update autocorrelator and find peak lag = current tempo */
            auco.newVal(onset);
            // Record largest value in (weighted) autocorrelation as it will be the tempo
            float aMax = 0.0f;
            int tempopd = 0;
            for (int i = 0; i < maxlag; ++i)
            {
                float acVal = (float)System.Math.Sqrt(auco.autoco(i));
                if (acVal > aMax)
                {
                    aMax = acVal;
                    tempopd = i;
                }
                // Store in array backwards, so it displays right-to-left, in line with traces
                acVals[maxlag - 1 - i] = acVal;
            }

            /* Calculate DP-ish function to update the best-score function */
            float smax = -999999;
            int smaxix = 0;
            alph = 100 * gThresh;
            // Consider all possible preceding beat times from 0.5 to 2.0 x current tempo period
            for (int i = tempopd / 2; i < System.Math.Min(colmax, 2 * tempopd); ++i)
            {
                // Objective function - this beat's cost + score to last beat + transition penalty
                float score = onset + scorefun[(now - i + colmax) % colmax] - alph * (float)System.Math.Pow(System.Math.Log((float)i / (float)tempopd), 2);
                // Keep track of the best-scoring predecessor
                if (score > smax)
                {
                    smax = score;
                    smaxix = i;
                }
            }

            scorefun[now] = smax;
            // Keep the smallest value in the score fn window as zero, by subtracting the min val
            float smin = scorefun[0];
            for (int i = 0; i < colmax; ++i)
                if (scorefun[i] < smin)
                    smin = scorefun[i];
            for (int i = 0; i < colmax; ++i)
                scorefun[i] -= smin;

            /* Find the largest value in the score fn window, to decide if we emit a blip */
            smax = scorefun[0];
            smaxix = 0;
            for (int i = 0; i < colmax; ++i)
            {
                if (scorefun[i] > smax)
                {
                    smax = scorefun[i];
                    smaxix = i;
                }
            }

            // dobeat array records where we actually place beats
            dobeat[now] = 0; // default is no beat this frame
            ++sinceLast;
            // If current value is largest in the array, probably means we're on a beat
            if (smaxix == now)
            {
                // Filter beats based on strength and timing (only emit strong beats with enough time in between)
                if (onset > gThresh && sinceLast > tempopd / 4)
                {
                    onBeat.Invoke();
                    blipDelay[0] = 1;
                    // Mark that we did actually detect a beat this frame
                    dobeat[now] = 1;
                    // Reset the counter of frames since last beat
                    sinceLast = 0;
                }
            }

            /* Update column index (for ring buffer) */
            if (++now == colmax)
                now = 0;
        }
    }

    public void changeCameraColor()
    {
        float r = Random.Range(0f, 1f);
        float g = Random.Range(0f, 1f);
        float b = Random.Range(0f, 1f);

        Color color = new Color(r, g, b);

        GetComponent<Camera>().clearFlags = CameraClearFlags.Color;
        Camera.main.backgroundColor = color;
    }

    public float getBandWidth()
    {
        return (2f / (float)bufferSize) * (samplingRate / 2f);
    }

    public int freqToIndex(int freq)
    {
        // Special case: freq is lower than the bandwidth of spectrum[0]
        if (freq < getBandWidth() / 2)
            return 0;
        // Special case: freq is within the bandwidth of spectrum[512]
        if (freq > samplingRate / 2 - getBandWidth() / 2)
            return (bufferSize / 2);
        // All other cases
        float fraction = (float)freq / (float)samplingRate;
        int i = (int)System.Math.Round(bufferSize * fraction);
        return i;
    }

    public void computeAverages(float[] data)
    {
        for (int i = 0; i < 12; i++)
        {
            float avg = 0;
            int lowFreq;
            if (i == 0)
                lowFreq = 0;
            else
                lowFreq = (int)((samplingRate / 2) / (float)System.Math.Pow(2, 12 - i));
            int hiFreq = (int)((samplingRate / 2) / (float)System.Math.Pow(2, 11 - i));
            int lowBound = freqToIndex(lowFreq);
            int hiBound = freqToIndex(hiFreq);
            for (int j = lowBound; j <= hiBound; j++)
            {
                avg += data[j];
            }
            avg /= (hiBound - lowBound + 1);
            averages[i] = avg;
        }
    }

    float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

    public float constrain(float value, float inclusiveMinimum, float inlusiveMaximum)
    {
        if (value >= inclusiveMinimum)
        {
            if (value <= inlusiveMaximum)
            {
                return value;
            }

            return inlusiveMaximum;
        }

        return inclusiveMinimum;
    }

    [System.Serializable]
    public class OnBeatEventHandler : UnityEngine.Events.UnityEvent { }

    [System.Serializable]
    public class OnSpectrumEventHandler : UnityEngine.Events.UnityEvent<float[]> { }


// class to compute an array of online autocorrelators
private class Autoco
    {
        private int del_length;
        private float decay;
        private float[] delays;
        private float[] outputs;
        private int indx;

        private float[] bpms;
        private float[] rweight;
        private float wmidbpm = 120f;
        private float woctavewidth;

        public Autoco(int len, float alpha, float framePeriod, float bandwidth)
        {
            woctavewidth = bandwidth;
            decay = alpha;
            del_length = len;
            delays = new float[del_length];
            outputs = new float[del_length];
            indx = 0;

            // calculate a log-lag gaussian weighting function, to prefer tempi around 120 bpm
            bpms = new float[del_length];
            rweight = new float[del_length];
            for (int i = 0; i < del_length; ++i)
            {
                bpms[i] = 60.0f / (framePeriod * (float)i);
                //Debug.Log(bpms[i]);
                // weighting is Gaussian on log-BPM axis, centered at wmidbpm, SD = woctavewidth octaves
                rweight[i] = (float)System.Math.Exp(-0.5f * System.Math.Pow(System.Math.Log(bpms[i] / wmidbpm) / System.Math.Log(2.0f) / woctavewidth, 2.0f));
            }
        }

        public void newVal(float val)
        {

            delays[indx] = val;

            // update running autocorrelator values
            for (int i = 0; i < del_length; ++i)
            {
                int delix = (indx - i + del_length) % del_length;
                outputs[i] += (1 - decay) * (delays[indx] * delays[delix] - outputs[i]);
            }

            if (++indx == del_length)
                indx = 0;
        }

        // read back the current autocorrelator value at a particular lag
        public float autoco(int del)
        {
            float blah = rweight[del] * outputs[del];
            return blah;
        }

        public float avgBpm()
        {
            float sum = 0;
            for (int i = 0; i < bpms.Length; ++i)
            {
                sum += bpms[i];
            }
            return sum / del_length;
        }
    }
}

