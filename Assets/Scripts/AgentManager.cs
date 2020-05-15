using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public float timeframe;
    public int populationSize;
    public GameObject prefab;

    public int[] layers = new int[3] { 8, 6, 5 };

    [Range(0.0001f, 1f)] public float MutationChance = 0.01f;
    [Range(0f, 1f)] public float MutationStrength = 0.5f;
    [Range(0.1f, 10f)] public float GameSpeed = 1f;

    private List<NeuralNetwork> nets;
    private List<Agent> agents;

    void Start()
    {
        if (populationSize % 2 != 0)
        {
            populationSize = 50;
        }

        InitNetworks();
        InvokeRepeating("CreateAgents", 0.1f, timeframe);
    }

    public void InitNetworks()
    {
        nets = new List<NeuralNetwork>();
        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(layers);
            net.Load("Assets/Save.txt");
            nets.Add(net);
        }
    }

    public void CreateAgents()
    {
        Time.timeScale = GameSpeed;
        if (agents != null)
        {
            for (int i = 0; i < agents.Count; i++)
            {
                GameObject.Destroy(agents[i].gameObject);
            }
            SortNetworks();
        }

        agents = new List<Agent>();
        for (int i = 0; i < populationSize; i++)
        {
            Agent agent = (Instantiate(prefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0))).GetComponent<Agent>();
            agent.net = nets[i];
            agents.Add(agent);
        }
    }

    public void SortNetworks()
    {
        for (int i = 0; i < populationSize; i++)
        {
            agents[i].UpdateFitness();
        }
        nets.Sort();
        nets[populationSize - 1].Save("Assets/Save.txt");
        for (int i = 0; i < populationSize / 2; i++)
        {
            nets[i] = nets[i + populationSize / 2].Copy(new NeuralNetwork(layers));
            nets[i].Mutate((int)(1 / MutationChance), MutationStrength);
        }
    }
}
