using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction {
  LEFT,
  RIGHT,
  UP,
  DOWN
}

[System.Serializable]
public struct Rule {
  public char c1;
  public char c2;
  public Direction dir;
}

[System.Serializable]
public struct Weight {
  public char tile;
  public float weight;
}

public class WFC : MonoBehaviour
{
  public List<char> tileset;
  public List<Rule> rules;
  public List<Weight> weight_list;
  public Dictionary<char, float> weights;
  public List<char> output;
  public List<List<char>> wave;
  public List<float> entropy;

  public int columns;
  public int rows;

  bool algorithm_start;
  bool autoresolve;

  // Start is called before the first frame update
  void Start()
  {
    weights = new Dictionary<char, float>();
    foreach(Weight w in weight_list) {
      weights[w.tile] = w.weight;
    }

  }

  // Update is called once per frame
  void Update()
  {
    if (Input.GetKeyDown(KeyCode.S)) {
      StartAlgorithm();
    }

    if(Input.GetKeyDown(KeyCode.A)) {
      autoresolve = !autoresolve;
    }

    if(algorithm_start) {
      if (Input.GetKeyDown(KeyCode.Space) || autoresolve) {
        // Observation
        int next_tile = GetNextCandidate();

        // if no available candidate, break cycle
        if (next_tile == -1) {
          Debug.Log("Finished");
          autoresolve = false;
        }
        else {
          SelectTile(next_tile);

          // Propagation
          Propagate(next_tile);

          PrintWave();
        }
      }
    }
  }

  void StartAlgorithm() {
    entropy = new List<float>();
    wave = new List<List<char>>();

    // Initialize wave to unobserved state
    for (int i = 0; i < rows * columns; i++) {
      wave.Add(new List<char>(tileset));
      entropy.Add(CalculateEntropy(i));
    }

    algorithm_start = true;
    Debug.Log("Initial state");
    PrintWave();
  }

  // shannon_entropy_for_square = log(sum(weight)) - (sum(weight * log(weight)) / sum (weight))
  float CalculateEntropy(int tile) {
    List<char> state = wave[tile];

    float result = 0f;
    float sum1 = 0f;
    float sum2 = 0f;

    if (state.Count <= 1) return 0f;

    for(int i = 0; i < state.Count; i++) {
        sum1 += weights[state[i]];
        sum2 += weights[state[i]] * Mathf.Log(weights[state[i]]);
    }

    result = Mathf.Log(sum1) - (sum2 / sum1);

    return result;
  }

  int GetNextCandidate() {
    int min_idx = -1;
    float min_value = 1000000000f;
    float epsilon = 0.00001f;

    int idx = Random.Range(0, entropy.Count);

    for(int i = 0; i < entropy.Count; i++) {
      if(Mathf.Abs(entropy[idx]) > epsilon && entropy[idx] < min_value) {
        min_value = entropy[idx];
        min_idx = idx;
      }

      idx = (idx + 1) % entropy.Count;
    }

    Debug.Log("Tile selected: " + min_idx);
    return min_idx;
  }

  void SelectTile(int tile) {
    char selected = 'X';
    List<char> candidates = wave[tile];

    float total_weight = 0f;

    for (int i = 0; i < candidates.Count; i++) {
      total_weight += weights[candidates[i]];
    }

    float prob = Random.Range(0f, total_weight);
    float accum_prob = 0f;

    for(int i = 0; i < candidates.Count; i++) {
      accum_prob += weights[candidates[i]];
      if(prob < accum_prob) {
        selected = candidates[i];
        break;
      }
    }

    if (selected == 'X')
      Debug.LogError("Couldn't select a tile");

    wave[tile].Clear();
    wave[tile].Add(selected);

    Debug.Log("Value selected: " + selected);
  }

  List<(int, Direction)> GetValidNeighbours(int tile) {
    List<(int, Direction)> neighbours = new List<(int, Direction)>();
    // calculate current tile's neighbours
    int up = tile - rows;
    int down = tile + rows;
    int left = tile - 1;
    int right = tile + 1;
    int row = tile / columns;

    // for each neighbour, check if it exists
    // if up is in the matrix range, enqueue
    if (up >= 0) {
      neighbours.Add((up, Direction.DOWN));
    }

    // if down is in the matrix range, enqueue
    if (down < wave.Count) {
      neighbours.Add((down, Direction.UP));
    }

    // if left is in the same row as next, enqueue
    if (left >= row * columns) {
      neighbours.Add((left, Direction.RIGHT));
    }

    // if right is in the same row as next, enqueue
    if (right < (row + 1) * columns) {
      neighbours.Add((right, Direction.LEFT));
    }

    return neighbours;
  }

  void Propagate(int tile) {
    List<int> visited = new List<int>();
    Stack<int> pending = new Stack<int>();

    pending.Push(tile);

    // begin propagation
    while (pending.Count != 0) {
      // get next tile to check
      int current = pending.Pop();

      List<char> curr_state = wave[current];
      List<(int, Direction)> next_neighbours = GetValidNeighbours(current);

      // for each neighbour of the current tile
      foreach((int, Direction) neighbour in next_neighbours) {
        List<char> copy_state = new List<char>(wave[neighbour.Item1]);
        // check each possible tile of that neighbour
        foreach (char neighbour_state in copy_state) {
          int idx = -1;
          // with each possible tile of the current state
          foreach (char curr_tile in curr_state) {
            // find if there is a rule for the neighbour's tile
            idx = rules.FindIndex(rule => rule.c1 == curr_tile && rule.c2 == neighbour_state && rule.dir == neighbour.Item2);

            // if a rule has been found, break the cycle
            if(idx != -1) {
              break;
            }
          }

          // if no rule has been found, collapse the current neighour tile
          if(idx == -1) {
            // collapse tile
            wave[neighbour.Item1].Remove(neighbour_state);

            // add neighbour to the stack
            pending.Push(neighbour.Item1);
          }
        }
      }
    }

    for(int i = 0; i < entropy.Count; i++) {
      entropy[i] = CalculateEntropy(i);
    }
  }

  void PrintWave() {
    string s = "";

    for (int i = 0; i < rows; i++) {
      for (int j = 0; j < columns; j++) {
        string values = new string(wave[i * columns + j].ToArray());
        s += values + "\t";
      }
      s += "\n";
      //Debug.Log(s);
    }

    Debug.Log(s);
  }

  void TestAlgorithm() {
    entropy = new List<float>();
    wave = new List<List<char>>();

    // Initialize wave to unobserved state
    for (int i = 0; i < rows * columns; i++) {
      wave.Add(new List<char>(tileset));
      entropy.Add(CalculateEntropy(i));
    }

    // Observation
    //int next_tile = GetNextCandidate();
    int next_tile = 12;
    //SelectTile(next_tile);

    wave[next_tile].Clear();
    wave[next_tile].Add('S');

    // Propagation
    Propagate(next_tile);

    PrintWave();

    next_tile = 7;
    wave[next_tile].Clear();
    wave[next_tile].Add('C');

    // Propagation
    Propagate(next_tile);

    PrintWave();
    
    next_tile = 2;
    wave[next_tile].Clear();
    wave[next_tile].Add('G');

    // Propagation
    Propagate(next_tile);

    PrintWave();
  }
}
