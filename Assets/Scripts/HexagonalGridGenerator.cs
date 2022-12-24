using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexagonalGridGenerator : MonoBehaviour
{
  public bool point_top_orientation_;
  public float rows_;
  public float columns_;
  public float hex_size_;
  public GameObject hexagon_prefab_;

  public List<GameObject> tilemap_;

  float height_;
  float width_;
  // Start is called before the first frame update
  void Start()
  {
    if(point_top_orientation_) {
      height_ = 2.0f * hex_size_;
      width_ = Mathf.Sqrt(3.0f) * hex_size_;
    }
    else {
      height_ = Mathf.Sqrt(3.0f) * hex_size_;
      width_ = 2.0f * hex_size_;
    }

    Debug.Log("Height = " + height_);
    Debug.Log("Width = " + width_);
    generateGrid();
  }

  private void Update()
  {
    if(Input.GetKeyDown(KeyCode.Space)) {
      generateGrid();
    }
  }

  float horizontalDistance() {
    if(point_top_orientation_) {
      return width_;
    }
    else {
      return 0.75f * width_;
    }
  }

  float verticalDistance() {
    if (point_top_orientation_)
    {
      return 0.75f * height_;
    }
    else
    {
      return height_;
    }
  }

  void resetTilemap() {
    foreach(GameObject tile in tilemap_) {
      Destroy(tile);
    }
    tilemap_.Clear();
  }

  void generateGrid() {
    resetTilemap();
    for (int row = 0; row < rows_; row++) {
      for(int col = 0; col < columns_; col++) {
        // Calculate position
        Vector3 pos = new Vector3(col * horizontalDistance(), 0.0f, row * verticalDistance());

        if (point_top_orientation_ == true && row % 2 != 0)
        {
          pos = new Vector3((col + 0.5f) * horizontalDistance(), 0.0f, row * verticalDistance());
        }
        if (point_top_orientation_ == false && col % 2 != 0) {
          pos = new Vector3(col * horizontalDistance(), 0.0f, (row + 0.5f) * verticalDistance());
        }

        // Calculate rotation
        Vector3 rot = Vector3.zero;
        if (point_top_orientation_ == false) {
          rot = new Vector3(0.0f, 30.0f, 0.0f);
        }
        GameObject tile = Instantiate(hexagon_prefab_, pos, Quaternion.Euler(rot));
        tilemap_.Add(tile);
      }
    }
  }
}
