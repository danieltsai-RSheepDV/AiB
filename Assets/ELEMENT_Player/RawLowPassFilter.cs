using UnityEngine;

public class RawLowPassFilter
{
    private double[] a;
    private double[] b;
    private double[] x;
    private double[] y;

    public RawLowPassFilter(double cutoffFrequency, double sampleRate, int order = 5)
    {
        int n = 2 * order;
        a = new double[n + 1];
        b = new double[n + 1];
        x = new double[n + 1];
        y = new double[n + 1];

        double nyquist = 0.5 * sampleRate;
        double normalCutoff = cutoffFrequency / nyquist;

        Butterworth(order, normalCutoff);
    }

    private void Butterworth(int order, double cutoff)
    {
        double[] A = new double[2 * order + 1];
        double[] d1 = new double[order];
        double[] d2 = new double[order];

        double theta = Mathf.PI * (float)cutoff;
        double st = Mathf.Sin((float)theta);
        double ct = Mathf.Cos((float)theta);

        for (int i = 0; i < order; i++)
        {
            double r = 2 * Mathf.Cos((float)((2 * i + 1) * Mathf.PI / (2 * order)));
            d1[i] = st * r;
            d2[i] = ct;
        }

        double g = 1.0;
        for (int i = 0; i < order; i++)
        {
            g = g * (1.0 - d1[i]) * (1.0 + d1[i]);
        }

        double factor = Mathf.Pow(10.0f, -3.0f / 20.0f);

        for (int i = 0; i <= 2 * order; i++)
        {
            A[i] = factor * g;
        }

        for (int i = 0; i <= 2 * order; i++)
        {
            if (i == 0)
            {
                b[i] = 1.0;
                a[i] = 1.0;
            }
            else
            {
                b[i] = -2 * d2[0];
                a[i] = 2 * d1[0];
            }
        }
    }

    public float[] ApplyFilter(float[] data)
    {
        int n = a.Length - 1;
        int dataLength = data.Length;
        float[] result = new float[dataLength];

        for (int i = 0; i < dataLength; i++)
        {
            x[0] = data[i];
            y[0] = (float)(b[0] * x[0]);

            for (int j = 1; j <= n; j++)
            {
                y[0] += (float)(b[j] * x[j] - a[j] * y[j]);
            }

            result[i] = (float)y[0];

            for (int j = n; j > 0; j--)
            {
                x[j] = x[j - 1];
                y[j] = y[j - 1];
            }
        }

        return result;
    }
}