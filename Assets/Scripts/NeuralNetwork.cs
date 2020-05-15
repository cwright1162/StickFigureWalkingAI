using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/*
 * The main Neural Network Class
 * Contains all the components and methods 
 * to create and train a neural network.
 */
public class NeuralNetwork : IComparable<NeuralNetwork>
{
    private int[] layers;
    private float[][] neurons;
    private float[][] biases;
    private float[][][] weights;

    // Tracks the overall fitness of the neural network
    public float fitness = 0;

    /*
     * Constructor
     * Initializes all arrays to store layers, neurons, biases, and weights.
     */
    public NeuralNetwork(int[] layers)
    {
        this.layers = new int[layers.Length];
        for (int i = 0; i <layers.Length; i++)
        {
            this.layers[i] = layers[i];
        }

        InitNeurons();
        InitBiases();
        InitWeights();
    }

    /*
     * Initialization method for neurons
     * Creates an empty array to store neurons
     */
    private void InitNeurons()
    {
        List<float[]> neuronsList = new List<float[]>();
        for (int i = 0; i < layers.Length; i++)
        {
            neuronsList.Add(new float[layers[i]]);
        }
        neurons = neuronsList.ToArray();
    }

    /*
     * Initialization method for biases
     * Creates an empty array to store biases
     */
    private void InitBiases()
    {
        List<float[]> biasList = new List<float[]>();
        for (int i = 0; i < layers.Length; i++)
        {
            float[] bias = new float[layers[i]];
            for (int j = 0; j < layers[i]; j++)
            {
                // Generate a random float value
                bias[j] = UnityEngine.Random.Range(-0.5f, 0.5f);
            }
            biasList.Add(bias);
        }
        biases = biasList.ToArray();
    }

    /*
     * Initialization method for weights
     * Creates an empty array to store weights
     */
    private void InitWeights()
    {
        List<float[][]> weightsList = new List<float[][]>();
        for (int i = 1; i < layers.Length; i++)
        {
            List<float[]> layerWeightsList = new List<float[]>();
            int neuronsInPrevLayer = layers[i - 1];
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float[] neuronWeights = new float[neuronsInPrevLayer];
                for (int k = 0; k < neuronsInPrevLayer; k++)
                {
                    // Generate a random float for the weights
                    neuronWeights[k] = UnityEngine.Random.Range(-0.5f, 0.5f);
                }
                layerWeightsList.Add(neuronWeights);
            }
            weightsList.Add(layerWeightsList.ToArray());
        }
        weights = weightsList.ToArray();
    }

    /*
     * Activation method to improve performance of the agent.
     * Activation method chosen is Tanh.
     * Tanh allows both positive and negative values.
     * Unity has a built in function for Tanh.
     */
    public float Activate(float value)
    {
        return (float)Math.Tanh(value);
    }

    /*
     * Feed Forward method for the Neural Network.
     * Takes all the input neurons from each layer,
     * and modifies the next layer with those input neurons, and so on.
     * Utilizes the activate method (Tanh by default) to improve performance.
     */
    public float[] FeedForward(float[] inputs)
    {
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }
        for (int i = 1; i < layers.Length; i++)
        {
            int layer = i - 1;
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float value = 0f;
                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    value += weights[i - 1][j][k] * neurons[i - 1][k];
                }
                neurons[i][j] = Activate(value + biases[i][j]);
            }
        }
        return neurons[neurons.Length - 1];
    }

    public int CompareTo(NeuralNetwork other)
    {
        if(other == null)
        {
            return 1;
        }
        else if (fitness > other.fitness)
        {
            return 1;
        }
        else if (fitness < other.fitness)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

    public NeuralNetwork Copy(NeuralNetwork neuralNet)
    {
        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                neuralNet.biases[i][j] = biases[i][j];
            }
        }
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    neuralNet.weights[i][j][k] = weights[i][j][k];
                }
            }
        }
        return neuralNet;
    }

    public void Save(string path)
    {
        File.Create(path).Close();
        StreamWriter writer = new StreamWriter(path, true);

        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                writer.WriteLine(biases[i][j]);
            }
        }

        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    writer.WriteLine(weights[i][j][k]);
                }
            }
        }
        writer.Close();
    }

    public void Load(string path)
    {
        TextReader tr = new StreamReader(path);
        int numberOfLines = (int)new FileInfo(path).Length;
        string[] listLines = new string[numberOfLines];
        int index = 1;

        for (int i = 1; i < numberOfLines; i++)
        {
            listLines[i] = tr.ReadLine();
        }
        tr.Close();
        if (new FileInfo(path).Length > 0)
        {
            for (int i = 0; i < biases.Length; i++)
            {
                for (int j = 0; j < biases[i].Length; j++)
                {
                    biases[i][j] = float.Parse(listLines[index]);
                    index++;
                }
            }

            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        weights[i][j][k] = float.Parse(listLines[index]);
                        index++;
                    }
                }
            }
        }
    }

    public void Mutate(int chance, float value)
    {
        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                biases[i][j] = (UnityEngine.Random.Range(0f, chance) <= 5) ? 
                    biases[i][j] += UnityEngine.Random.Range(-value, value) : biases[i][j];
            }
        }
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    weights[i][j][k] = (UnityEngine.Random.Range(0f, chance) <= 5) ? 
                        weights[i][j][k] += UnityEngine.Random.Range(-value, value) : weights[i][j][k];
                }
            }
        }
    }
}
