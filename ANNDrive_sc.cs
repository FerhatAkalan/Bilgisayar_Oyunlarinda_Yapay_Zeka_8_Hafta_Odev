using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ANNDrive : MonoBehaviour {
    // Yapay sinir ağı (Artificial Neural Network) nesnesi
    ANN ann;
    // Işınların görünürlük mesafesi
    public float visibleDistance = 200.0f;
    // Eğitim için epoch sayısı
    public int epochs = 50000;
    // Araç hareket ve dönüş hızları
    public float speed = 50.0F;
    public float rotationSpeed = 100.0F;
    // Eğitim durumu ve eğitim ilerleme oranı
    bool trainingDone = false;
    float trainingProgress = 0;
    // Toplam hata (SSE - Sum of Squared Errors)
    double sse = 0;
    double lastSSE = 1; 
    // Araç hareket ve dönüş girdileri
    public float translation;
    public float rotation;
    // Ağırlıkların dosyadan yüklenip yüklenmeyeceğini kontrol eder
    public bool loadFromFile = false;
	
    // Use this for initialization
	void Start () {
        // ANN parametreleri: 5 giriş, 2 çıkış, 1 gizli katman, 10 nöron, 0.5 öğrenme oranı
        ann = new ANN(5,2,1,10,0.5);
        // Eğer ağırlıklar dosyadan yüklenmişse eğitim atlanır
		if(loadFromFile)
        {
			LoadWeightsFromFile();
			trainingDone = true;
        }
        else
            // Eğitim veri setini yükler
        	StartCoroutine(LoadTrainingSet());
	}
    // Eğitim durumunu ve diğer bilgileri göstermek için GUI
    void OnGUI()
    {
        GUI.Label (new Rect (25, 25, 250, 30), "SSE: " + lastSSE);
        GUI.Label (new Rect (25, 40, 250, 30), "Alpha: " + ann.alpha);
        GUI.Label (new Rect (25, 55, 250, 30), "Trained: " + trainingProgress);
    }
    // Eğitim veri setini yükleme
    IEnumerator LoadTrainingSet()
    {
        
        string path = Application.dataPath + "/trainingData2.txt";
        string line;
        // Eğer veri seti dosyası mevcutsa işlem başlar
        if(File.Exists(path))
        {
            int lineCount = File.ReadAllLines(path).Length;
            StreamReader tdf = File.OpenText(path);
            List<double> calcOutputs = new List<double>();
            List<double> inputs = new List<double>();
            List<double> outputs = new List<double>();
            // Belirtilen epoch sayısı kadar eğitim
            for(int i = 0; i < epochs; i++)
            { 
                //set file pointer to beginning of file
                sse = 0;
                tdf.BaseStream.Position = 0;
                string currentWeights = ann.PrintWeights();
                while((line = tdf.ReadLine()) != null)  
                {  
                    string[] data = line.Split(';');
                    Debug.Log("Dosyadan okuma: " + data[0] + " " + data[1] + " " + data[2] + 
                                " " + data[3] + " " + data[4] + " " + data[5] + " " + data[6]);
                    //if nothing to be learned ignore this line
                    float thisError = 0;
                    // Ignore data with 0 translation and rotation values
                    if(System.Convert.ToDouble(data[5]) != 0 && System.Convert.ToDouble(data[6]) != 0)
                    {
                        inputs.Clear();
                        outputs.Clear();
                        inputs.Add(System.Convert.ToDouble(data[0]));
                        inputs.Add(System.Convert.ToDouble(data[1]));
                        inputs.Add(System.Convert.ToDouble(data[2]));
                        inputs.Add(System.Convert.ToDouble(data[3]));
                        inputs.Add(System.Convert.ToDouble(data[4]));

                        Debug.Log("Dosyadan okuma 2: " + inputs[0] + " " + inputs[1] + " " + inputs[2] + 
                                " " + inputs[3] + " " + inputs[4]);

                        // Convert -1,1 ranged values to 0,1 range (normalize)
                        double o1 = Map(0, 1, -1, 1, System.Convert.ToSingle(data[5]));
                        outputs.Add(o1);
                        double o2 = Map(0, 1, -1, 1, System.Convert.ToSingle(data[6]));
                        outputs.Add(o2);

                        Debug.Log("Dosyadan okuma 3: " + outputs[0] + " " + outputs[1]);
                        // Yapay sinir ağını eğitir
                        calcOutputs = ann.Train(inputs,outputs); 
                        // Hata değerini hesaplar
                        thisError = ((Mathf.Pow((float)(outputs[0] - calcOutputs[0]),2) +
                            Mathf.Pow((float)(outputs[1] - calcOutputs[1]),2)))/2.0f;
                    }
                    sse += thisError;
                } 
                trainingProgress = (float)i/(float)epochs;
                sse /= lineCount;
                
                //if sse isn't better then reload previous set of weights
                //and decrease alpha
                if(lastSSE < sse)
                {
                	ann.LoadWeights(currentWeights);
                	ann.alpha = Mathf.Clamp((float)ann.alpha - 0.001f,0.01f,0.9f);
                }
                else //increase alpha
                {
                	ann.alpha = Mathf.Clamp((float)ann.alpha + 0.001f,0.01f,0.9f);
                	lastSSE = sse;
                }

                yield return null;
            }
        }
        trainingDone = true;
        // Eğitim tamamlandıktan sonra ağırlıklar kaydedilir
        if (!loadFromFile)
            SaveWeightsToFile();
    }
    // Ağırlıkları dosyaya kaydeder
    void SaveWeightsToFile()
    {
        string path = Application.dataPath + "/weights.txt";
        StreamWriter wf = File.CreateText(path);
        wf.WriteLine (ann.PrintWeights());
        wf.Close();
    }
    // Ağırlıkları dosyadan yükler
    void LoadWeightsFromFile()
    {
    	string path = Application.dataPath + "/weights.txt";
    	StreamReader wf = File.OpenText(path);

        if(File.Exists(path))
        {
        	string line = wf.ReadLine();
        	ann.LoadWeights(line);
        }
    }
    // Haritalama fonksiyonu: Değerleri farklı bir aralığa dönüştürür
    float Map (float newfrom, float newto, float origfrom,float origto, float value) 
    {
    	if (value <= origfrom)
        	return newfrom;
    	else if (value >= origto)
        	return newto;
    	return (newto - newfrom) * ((value - origfrom) / (origto - origfrom)) + newfrom;
	}
    // Yuvarlama fonksiyonu
    float Round(float x) 
    {   
        return (float)System.Math.Round(x, System.MidpointRounding.AwayFromZero) / 2.0f;
    }
    // Güncelleme döngüsü: Eğitim tamamlandıktan sonra çalışır
    void Update() {
        if(!trainingDone) return;

        List<double> calcOutputs = new List<double>();
        List<double> inputs = new List<double>();
        List<double> outputs = new List<double>();

        //raycasts
        RaycastHit hit;
        float fDist = 0, rDist = 0, lDist = 0, r45Dist = 0, l45Dist = 0;

        //forward
        if (Physics.Raycast(transform.position, this.transform.forward, out hit, visibleDistance))
        {
            fDist = 1-Round(hit.distance/visibleDistance);
        }
        //right
        if (Physics.Raycast(transform.position, this.transform.right, out hit, visibleDistance))
        {
            rDist = 1-Round(hit.distance/visibleDistance);
        }
        //left
        if (Physics.Raycast(transform.position, -this.transform.right, out hit, visibleDistance))
        {
            lDist = 1-Round(hit.distance/visibleDistance);
        }
        //right 45
        if (Physics.Raycast(transform.position, 
                            Quaternion.AngleAxis(-45, Vector3.up) * this.transform.right, 
                            out hit, visibleDistance))
        {
            r45Dist = 1-Round(hit.distance/visibleDistance);
        }
        //left 45
        if (Physics.Raycast(transform.position, 
                            Quaternion.AngleAxis(45, Vector3.up) * -this.transform.right, 
                            out hit, visibleDistance))
        {
            l45Dist = 1-Round(hit.distance/visibleDistance);
        }
        // Giriş verilerini ekler
        inputs.Add(fDist);
        inputs.Add(rDist);
        inputs.Add(lDist);
        inputs.Add(r45Dist);
        inputs.Add(l45Dist);
        outputs.Add(0);
        outputs.Add(0);
        // Yapay sinir ağı çıktısını hesaplar
        calcOutputs = ann.CalcOutput(inputs,outputs);
        float translationInput = Map(-1,1,0,1,(float) calcOutputs[0]);
        float rotationInput = Map(-1,1,0,1,(float) calcOutputs[1]);
        // Aracı hareket ettirir
        translation = translationInput * speed * Time.deltaTime;
        rotation = rotationInput * rotationSpeed * Time.deltaTime;
        this.transform.Translate(0, 0, translation);
        this.transform.Rotate(0, rotation, 0);        
    }
}
