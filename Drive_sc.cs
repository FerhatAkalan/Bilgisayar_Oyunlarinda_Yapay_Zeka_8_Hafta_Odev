using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Drive_sc : MonoBehaviour
{
    // Aracın hareket hızı
    [SerializeField]
    float speed = 50.0f;
    // Aracın dönüş hızı
    [SerializeField]
    float rotationSpeed = 100.0f;
    // Görünürlük mesafesi (ışınların mesafesi)
    [SerializeField]
    float visibleDistance = 200.0f;
    // Toplanan eğitim verilerini saklayan liste
    List<string> collectedTrainingData = new List<string>();
    StreamWriter tdf;
    // İleri/geri hareket için giriş değeri
    [SerializeField]
    float translationInput;

    void Start()
    {
        // Eğitim verilerini kaydetmek için dosya oluşturulur
        string path = Application.dataPath + "/trainingData2.txt";
        tdf = File.CreateText(path);
    }

    void OnApplicationQuit()
    {
        // Eğitim verileri dosyaya yazılır
        foreach (string td in collectedTrainingData)
        {
            tdf.WriteLine(td);
        }
        tdf.Close();
    }
    // Verileri yuvarlamak için kullanılan yardımcı fonksiyon
    float Round(float x)
    {
        return (float) System.Math.Round(x, System.MidpointRounding.AwayFromZero) / 2.0f;
    }

    void Update()
    {
        // İleri/geri hareket girdisini alır
        translationInput = Input.GetAxis("Vertical");
        // Dönüş girdisini alır
        float rotationInput = Input.GetAxis("Horizontal");
        // Hareket ve dönüş değerlerini hesaplar
        float translation = translationInput * speed * Time.deltaTime;
        float rotation = rotationInput * rotationSpeed * Time.deltaTime;
        // Aracı hareket ettirir
        transform.Translate(0, 0, translation);
        // Aracı döndürür
        transform.Rotate(0, rotation, 0);
        // Görselleştirilen ışınlar (raycast) için çizim
        Debug.DrawRay(transform.position, this.transform.forward * visibleDistance, Color.red);
        Debug.DrawRay(transform.position, this.transform.right * visibleDistance, Color.blue);
        Debug.DrawRay(transform.position, -this.transform.right * visibleDistance, Color.blue);
        Debug.DrawRay(transform.position, Quaternion.AngleAxis(-45, Vector3.up) * this.transform.right * visibleDistance, Color.green);
        Debug.DrawRay(transform.position, Quaternion.AngleAxis(45, Vector3.up) * -this.transform.right * visibleDistance, Color.green);
        // Raycast verilerini saklamak için değişkenler
        RaycastHit hit;
        float fDist = 0, rDist = 0, lDist = 0, r45Dist = 0, l45Dist = 0;

        // Forward
        if (Physics.Raycast(transform.position, this.transform.forward, out hit, visibleDistance))
        {
            fDist = 1 - Round(hit.distance/visibleDistance);
        }

        // Right
        if (Physics.Raycast(transform.position, this.transform.right, out hit, visibleDistance))
        {
            rDist = 1 - Round(hit.distance/visibleDistance);
        }

        // Left
        if (Physics.Raycast(transform.position, -this.transform.right, out hit, visibleDistance))
        {
            lDist = 1 - Round(hit.distance/visibleDistance);
        }

        // Forward right 45 degrees
        if (Physics.Raycast(transform.position, 
                            Quaternion.AngleAxis(-45, Vector3.up) * this.transform.right, 
                            out hit, visibleDistance))
        {
            r45Dist = 1 - Round(hit.distance/visibleDistance);
        }

        // Forward left 45 degrees
        if (Physics.Raycast(transform.position, 
                            Quaternion.AngleAxis(45, Vector3.up) * -this.transform.right, 
                            out hit, visibleDistance))
        {
            l45Dist = 1 - Round(hit.distance/visibleDistance);
        }
        // Eğitim verisi oluşturma (mesafeler ve giriş değerleri)
        string td = fDist + "," + rDist + "," + lDist + "," + r45Dist + "," + l45Dist + "," + 
                    Round(translationInput) + "," + Round(rotationInput);
        // Aynı veriyi tekrar eklememek için kontrol
        if (!collectedTrainingData.Contains(td))
        {
            collectedTrainingData.Add(td);// Veriyi listeye ekle
        }
    }
}