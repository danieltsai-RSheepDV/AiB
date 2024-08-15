using UnityEngine;

public class RawHighPassFilter
{
    private float a0, a1, a2, b1, b2;
    private float x1, x2, y1, y2;

    public RawHighPassFilter(float cutoffFrequency, float sampleRate, float q = 0.707f)
    {
        float omega = 2 * Mathf.PI * cutoffFrequency / sampleRate;
        float alpha = Mathf.Sin(omega) / (2 * q);
        float cos_omega = Mathf.Cos(omega);
        
        // Calculate filter coefficients
        float norm = 1 / (1 + alpha);
        a0 = ((1 + cos_omega) / 2) * norm;
        a1 = -(1 + cos_omega) * norm;
        a2 = ((1 + cos_omega) / 2) * norm;
        b1 = -2 * cos_omega * norm;
        b2 = (1 - alpha) * norm;

        // Initialize memory variables
        x1 = x2 = y1 = y2 = 0.0f;
    }

    public float[] ApplyFilter(float[] data)
    {
        float[] result = new float[data.Length];

        for (int i = 0; i < data.Length; i++)
        {
            float input = data[i];
            float output = a0 * input + a1 * x1 + a2 * x2 - b1 * y1 - b2 * y2;

            x2 = x1;
            x1 = input;
            y2 = y1;
            y1 = output;

            result[i] = output;
        }

        return result;
    }
}