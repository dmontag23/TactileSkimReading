using BrailleIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;

namespace SkimReadingStudy
{
    // holds the content of the pages of the individual papers
    class Page
    {
        private Dictionary<String, String> namesAndText = new Dictionary<String, String>();     // holds the titles of the text along with the associated text for each page
        OrderedDictionary viewRangesOnDevice = new OrderedDictionary();                         // holds the view ranges displayed on the device for the page
        
        // constructor - the construction of a "page" object is filled with different content depending on the paper name and page number 
        // namesAndText associates the text of a section with a title
        // viewRangesOnDevice creates a view range for the section with the specified title from namesAndText
        // the order in which view ranges are added to "viewRangesOnDevice" is the sequential order of the paragraphs on the paper
        public Page(String nameOfPaper, int pageNum)
        {
            
            #region Learning Paper 1 - Raconteur

            // page 1 - Raconteur
            if (nameOfPaper == "paper1" && pageNum == 1)
            {
                namesAndText.Add("title", "Raconteur: From Intent to Stories  ");
                namesAndText.Add("author", "Pei-Yu Chi, Henry Lieberman  MIT Media Laboratory  20 Ames St., Cambridge, MA, USA  {peggychi, lieber}@media.mit.edu  ");
                namesAndText.Add("abstract1", "ABSTRACT  When editing a story from a large collection of media, such  as photos and video clips captured from daily life, it is not  always easy to understand how particular scenes fit into the  intent for the overall story. Especially for novice editors,  there is often a lack of coherent connections between scenes,  making it difficult for the viewers to follow the story.   ");
                namesAndText.Add("abstract2", "In this paper, we present Raconteur, a story editing system  that helps users assemble coherent stories from media  elements, each annotated with a sentence or two in  unrestricted natural language.  It uses a Commonsense  knowledge base, and the AnalogySpace Commonsense  reasoning technique. Raconteur focuses on finding story  analogies – different elements illustrating the same overall  'point', or independent stories exhibiting similar narrative  structures.    ");
                namesAndText.Add("keywords", "Author Keywords  Storytelling, media editing, story goal, story analogy,  commonsense computing, video, photograph.  ");
                namesAndText.Add("classification", "ACM Classification Keywords  H5.m. Information interfaces and presentation (e.g., HCI):  Miscellaneous.   ");
                namesAndText.Add("general", "General Terms  Design, Human Factors  ");
                namesAndText.Add("1.1", "INTRODUCTION  When presenting or editing a large set of material, such as  photos and video clips captured from daily life, it is not easy  to understand how particular scenes fit into the overall story.  Most people therefore choose to present the story by events  in chronological order [6], or by location or characters.  Although there is software for automating categorization or  suggesting keyword tags, it is still challenging to create a  coherent higher-level presentation that tells an entertaining  story. Novice users often do not pay attention to the 'point'  being made by showing a given scene, or provide meaningful  connections between scenes, making it difficult for the\nContinued on next column");
                namesAndText.Add("copyright", "  Permission to make digital or hard copies of all or part of this work for  personal or classroom use is granted without fee provided that copies are  not made or distributed for profit or commercial advantage and that copies  bear this notice and the full citation on the first page. To copy otherwise,  or republish, to post on servers or to redistribute to lists, requires prior  specific permission and/or a fee.  IUI’10, February 7–10, 2010, Hong Kong, China.  Copyright 2010 ACM  978-1-60558-515-4/10/02...$10.00.  ");
                namesAndText.Add("1.1cont", "viewers to follow the story. We believe that an intelligent  interface that provides assistance in relating the concrete  elements of the scene to the overall story intent, will result in  more effective story composition.  ");
                namesAndText.Add("1.2", "We present Raconteur, a story editing system that helps users  think about material in a story, by showing related scenes or  other stories with similar goals. The word “raconteur”, by  definition, is a person who is skilled in relating stories and  anecdotes meaningfully. Similarly, our system tries to  understand the narrative goal presented by the user, and find  analogous story elements related to the goal.  ");
                namesAndText.Add("1.3", "We aim to create a system that helps a user tell a story by  selecting a sequence of media items from a corpus of video,  stills, narrations, and other media. We are assuming that each  media element is annotated with a sentence or two in  unrestricted natural language. The annotation may describe  people, events, actions, and intent of the scene. Some  annotations may be generated by metadata, transcription of  audio, or other means.   ");
                namesAndText.Add("1.4", "A user at first presents a story goal in natural language, for  example, “a 4-hour biking challenge on Cape Cod”. The  objective of the system is to provide a selection of possible  matches of the annotated media elements to the story goal,  that best help to tell the story. Note that this objective is not  the same as simple search, keyword matching, subject  relevance, or other conventional retrieval problems.   ");
                namesAndText.Add("1.5", "The tools we use for doing this are a large Commonsense  knowledge base, Open Mind Common Sense;  state-of-the art  natural language parsing; and our own unique AnalogySpace  inference technique for analogical reasoning. We can only  provide a brief description of the knowledge base and  inference in this paper; we refer the reader to the references  for more detailed explanation of the tools.   ");
                namesAndText.Add("1.6", "For the 'biking challenge' example, the system is able to  suggest video clips that support relevant themes, such as  anticipation and worries, preparation, difficulties, and,  finally, the result of success or failure of the trip.   ");
                namesAndText.Add("1.7", "Analogous Story Thinking  We are inspired by how humans understand stories using  analogies, which are partial similarities between different  situations that support further inferences [4]. Schank  proposed the idea of “story skeleton” to explain how we  construct and comprehend a story under a certain structure to  communicate with each other [11].  ");
                viewRangesOnDevice.Add("title", CreateViewRange(new Rectangle(10, 5, 6, 50), "rect"));
                viewRangesOnDevice.Add("author", CreateViewRange(new Rectangle(19, 22, 7, 16), "rect"));           
                viewRangesOnDevice.Add("abstract1", CreateViewRange(new Rectangle(29, 35, 10, 20), "head"));
                viewRangesOnDevice.Add("abstract2", CreateViewRange(new Rectangle(42, 35, 12, 20), "rect"));
                viewRangesOnDevice.Add("keywords", CreateViewRange(new Rectangle(57, 35, 3, 20), "rect"));
                viewRangesOnDevice.Add("classification", CreateViewRange(new Rectangle(62, 35, 3, 20), "rect"));
                viewRangesOnDevice.Add("general", CreateViewRange(new Rectangle(67, 35, 3, 20), "rect"));
                viewRangesOnDevice.Add("1.1", CreateViewRange(new Rectangle(73, 35, 17, 20), "head"));
                viewRangesOnDevice.Add("copyright", CreateViewRange(new Rectangle(93, 35, 9, 20), "rect"));
                viewRangesOnDevice.Add("1.1cont", CreateViewRange(new Rectangle(29, 5, 5, 20), "rect"));
                viewRangesOnDevice.Add("1.2", CreateViewRange(new Rectangle(37, 5, 9, 20), "rect"));
                viewRangesOnDevice.Add("1.3", CreateViewRange(new Rectangle(49, 5, 9, 20), "rect"));
                viewRangesOnDevice.Add("1.4", CreateViewRange(new Rectangle(61, 5, 9, 20), "rect"));
                viewRangesOnDevice.Add("1.5", CreateViewRange(new Rectangle(73, 5, 8, 20), "rect"));
                viewRangesOnDevice.Add("1.6", CreateViewRange(new Rectangle(84, 5, 5, 20), "rect"));
                viewRangesOnDevice.Add("1.7", CreateViewRange(new Rectangle(92, 5, 9, 20), "head"));
            }

            // page 2 - Raconteur
            if (nameOfPaper == "paper1" && pageNum == 2)
            {
                namesAndText.Add("1.8", "For example, a camping, hiking, and biking trip may include  challenges (putting a tent up, going through a long and steep  path), difficulties or worries (cannot assemble the tent poles,  unable to finish the path, get lost on the way, bad weather),  and enjoyable experiences (learning the setup, arriving a new  place, meeting new friends) of a physical activity with a  group of people. Similar points may be presented again and  again through the process of developing a story. Superficially  different events may illustrate analogous themes, so the  ability to make analogies helps tell a story in a coherent way  to the audience.  ");
                namesAndText.Add("2.1", "FORMATIVE USER STUDY  Before we present the Raconteur system itself, we describe a  formative study to see if users would find presentation of  analogous story elements helpful in story construction. In  [12], we presented a more extensive user study reporting  concrete experience with a previously implemented system  for story construction via Commonsense knowledge. The  present study is concerned with the value of the new  analogical inference. We designed a story-editing interface  that shows both the raw set of selected material and the  analogous elements we found.  ");
                namesAndText.Add("2.2", "Experimental setup  We asked one participant to collect media (photos or videos)  documenting her life experiences for three months. We  observed the 30 collected stories, selected and compared  similar topics, and specified the key shots in each story.   ");
                namesAndText.Add("2.3", "We summarize three main categories: 1) stories with a clear  procedure as a story pattern, e.g. birthday parties that people  sing the 'Happy Birthday' song, make wishes, cut the cake,  etc.; 2) Stories without a clear procedure but with certain  expected events; 3) Stories without a clear procedure and  expected events, e.g. a camping, hiking, and biking trips that  include difficult challenges and new experience of an  activity. We chose one story from each of these categories as  test cases, including “Hsien’s birthday party with a potluck  dinner”, “Mike’s commencement party for his first master's  degree”, and “A 4-hour biking challenge in Cape Cod”. The  collector annotated each media element with a sentence or  two in English.   ");
                namesAndText.Add("2.4", "5 participants were invited, including 3 males and 2 females,  aged between 20-30, experienced with digital media.  Participants were asked to edit stories for sharing with their  friends. The facilitator first helped them familiarize  themselves with the test cases, and then introduced our  editing interface and conducted the 3 editing sessions.   ");
                namesAndText.Add("2.5", "Results  We found that when the size of the corpus was large and the  story was relatively complex (test case 1&3), presenting the  analogous story helped users follow a story pattern better.  Especially for test case 3 (a biking trip), participants found  the story complex for them to edit, and reported the  analogous examples helped them to design the story  development. Most participants spent considerable time on\nContinued on next column");
                namesAndText.Add("2.5cont", "observing the similarity of story content. One participant  said, “It was interesting to see how the system presented a  new perspective to the story I wanted to tell”; another  explained, “The system helped me rethink the similarity and  differences between experiences, which I would rarely think  to do from just browsing a bunch of files.” These findings  encouraged us that the analogical reasoning mechanism  would prove useful to users in story construction.  ");
                namesAndText.Add("3.1", "DESIGN OF THE RACONTEUR SYSTEM  In this section we present a concrete example of a story of a  vacation trip from the collected story base, along with the  description of our knowledge representation and inference  methods in Raconteur, and the user interface.   ");
                namesAndText.Add("3.2", "Finding Elements Analogous to the Story Goal  A user starts by inputting a story goal, in unrestricted natural  language. A simple example is, “a 4-hour biking challenge  on Cape Cod”. First of all, we need to determine the possible  patterns to present the goal. Table 1 lists sample narrative  goals and media annotations that Raconteur finds for this  Cape Cod biking trip example.   ");
                namesAndText.Add("3.3", "Both goal statements and media item annotations are  processed using conventional natural language tools such as  part of speech tagging. The result is related to knowledge  stored in our Open Mind Common Sense knowledge base,  and the ConceptNet semantic network [9]. Assertions from  that knowledge base relevant to the concept 'challenge', after  excluding anything that has no connection to 'biking'   include,\nContinued on next page");
                namesAndText.Add("tab1", "Table 1. Multiple story units with similar patterns that Raconteur finds. The number indicates the elements in the same set of patterns.");
                viewRangesOnDevice.Add("1.8", CreateViewRange(new Rectangle(4, 35, 19, 20), "rect"));
                viewRangesOnDevice.Add("2.1", CreateViewRange(new Rectangle(26, 35, 20, 20), "head"));
                viewRangesOnDevice.Add("2.2", CreateViewRange(new Rectangle(49, 35, 7, 20), "head"));
                viewRangesOnDevice.Add("2.3", CreateViewRange(new Rectangle(59, 35, 23, 20), "rect"));
                viewRangesOnDevice.Add("2.4", CreateViewRange(new Rectangle(85, 35, 6, 20), "rect"));
                viewRangesOnDevice.Add("2.5", CreateViewRange(new Rectangle(94, 35, 8, 20), "head"));
                viewRangesOnDevice.Add("2.5cont", CreateViewRange(new Rectangle(4, 5, 14, 20), "rect"));
                viewRangesOnDevice.Add("3.1", CreateViewRange(new Rectangle(21, 5, 8, 20), "head"));
                viewRangesOnDevice.Add("3.2", CreateViewRange(new Rectangle(32, 5, 12, 20), "head"));
                viewRangesOnDevice.Add("3.3", CreateViewRange(new Rectangle(47, 5, 13, 20), "head"));
                viewRangesOnDevice.Add("tab1", CreateViewRange(new Rectangle(63, 5, 39, 20), "tab"));
            }

            // page 3 - Raconteur
            if (nameOfPaper == "paper1" && pageNum == 3)
            {
                namesAndText.Add("3.3cont", "• Desires (challenge, anticipate something)  • MotivatedByGoal (challenge, test oneself)  • HasProperty (challenge, difficulty)  • Causes (challenge, success)  • Causes (challenge, failure)  ");
                namesAndText.Add("3.4", "One pattern for presenting a challenge involves  “anticipations and worries”, “preparation”, “difficulties” and  “results” (successes or failures). When Raconteur finds more  than one pattern to present the goal, multiple patterns can be  considered.  ");
                namesAndText.Add("3.5", "Analogical Inference in Raconteur  Second, Raconteur analyzes the annotation of each media  element in natural language, and makes analogical  inferences. It uses the AnalogySpace Commonsense  reasoning technique [13].  ");
                namesAndText.Add("3.6", "AnalogySpace represents the entire space of OMCS's  knowledge through a sparse matrix whose rows are  ConceptNet concepts (noun phrases and verb phrases), and  whose columns are features, one-argument predicates that  can be applied to those concepts. A feature generally consists  of one of 20 or so two-place relations (kind_of, part_of, etc.)  together with another concept. Inference is performed by  Principal Component Analysis on this matrix, using the  Singular Value Decomposition. This transforms the space by  finding those axes that best account for the variation in the  matrix. These axes are often semantically meaningful. The  reason this is good for computing analogy is that concepts  that have similar Commonsense assertions true about them  wind up close to each other in the transformed space. Unlike\nContinued on next column");
                namesAndText.Add("3.7", "logical approaches to analogy, it is computationally efficient,  and tolerant of vagueness, noise, redundancy, and  contradiction.   ");
                namesAndText.Add("3.8", "For example, A1 in Table 1 with descriptions of “Cape Cod”,  “stunning”, “famous”, “vacation”, and “biking” infers this  piece of material indicates the user’s anticipation of the trip;  P1 infers the preparation including having brunch, and  renting a bike; D1 explains the difficulty of finding the way  to avoid getting lost; then R1 shows the excitement of the  arrival.   ");
                namesAndText.Add("3.9", "Finally, by finding the elements related to the story goal, we  provide different perspectives on telling a story within a set  of material. If there is more than one generated pattern that  matches the story goal, Raconteur will go through this  process to different patterns and present one that contains the  most analogous elements in the set.  ");
                namesAndText.Add("3.10", "Raconteur User Interface  The final results of analogous story elements will be shown  through the web-based user interface as Figure 1. A user can  see the unorganized, sequential material in chronological  order as in Figure 1a. The user decides a story goal in  English, and then the analogous elements will be shown, as  in Figure 1b. The user can drag and drop photos or video  clips as desired to create a story.  ");
                namesAndText.Add("3.11", "Discussion  We’d also like to discuss how Raconteur might enable new  kinds of storytelling activities. Our model encourages users  to think about the story goal instead of directly composing  individual elements. To help participants reason how the\nContinued on next page");
                namesAndText.Add("fig1", " Figure 1. Raconteur interface: (a) the upper part presents the raw material of the unorganized story, and provides the editing  interface for users to decide the story goal and the sequence of scenes; (b) the lower part shows the sets of analogous story  elements in a pattern that matches the story goal.  ");
                viewRangesOnDevice.Add("3.3cont", CreateViewRange(new Rectangle(4, 35, 8, 20), "rect"));
                viewRangesOnDevice.Add("3.4", CreateViewRange(new Rectangle(15, 35, 8, 20), "rect"));
                viewRangesOnDevice.Add("3.5", CreateViewRange(new Rectangle(26, 35, 9, 20), "head"));
                viewRangesOnDevice.Add("3.6", CreateViewRange(new Rectangle(38, 35, 24, 20), "rect"));
                viewRangesOnDevice.Add("3.7", CreateViewRange(new Rectangle(4, 5, 4, 20), "rect"));
                viewRangesOnDevice.Add("3.8", CreateViewRange(new Rectangle(11, 5, 11, 20), "rect"));
                viewRangesOnDevice.Add("3.9", CreateViewRange(new Rectangle(25, 5, 10, 20), "rect"));
                viewRangesOnDevice.Add("3.10", CreateViewRange(new Rectangle(38, 5, 14, 20), "rect"));
                viewRangesOnDevice.Add("3.11", CreateViewRange(new Rectangle(55, 5, 7, 20), "rect"));
                viewRangesOnDevice.Add("fig1", CreateViewRange(new Rectangle(65, 5, 37, 50), "img"));
            }

            // page 4 - Raconteur
            if (nameOfPaper == "paper1" && pageNum == 4)
            {
                namesAndText.Add("3.11cont", "pattern and the results were generated, we highlight the  keywords that relate to system inferences, allowing users to  revise the suggested pattern and explore the space of results.  Furthermore, to make the storytelling process closer to the  experience of daily conversation, we are also considering  integrating our technique in a chat scenario. The storyteller  can interact with a partner to talk about the stories, and  Raconteur can suggest both story topics and media elements.   ");
                namesAndText.Add("4.1", "RELATED WORK  Cooper et. al. designed a similarity-based analysis to cluster  photos by timestamps and content [3]. Joshi and Luo  presented a method to infer events and activities from  geographical information [5]. However, most of the research  work on automatic media organization focuses on analyzing  the basic attributes such as time and location; few of them  consider the overall story development and story thinking  with digital media.   ");
                namesAndText.Add("4.2", "Another emerging research area is to interact with digital  media on the level of story composition. ARIA is a software  agent that dynamically retrieves related photos based on the  content of an email or web page [8]. Barry presented a  system that presents contextual information during the  process of video capture [1]. The closest system to the  present one is Storied Navigation [12], which shares the goal  of composing stories from annotated media clips. Our work  here differs in the use of the analogy inference technique, and  is focused on instantiating narrative goals directly through  analogical inference to create better story structures.   ");
                namesAndText.Add("4.3", "We are also aware of several relevant narrative systems such  as the storytelling and planning system “Universe,” which  models the story structure as a set of hierarchical plans and  generates plot outlines based on the author’s story goal [7];  Riedl and León’s story analogous generation system is able  to transform existing stories to a novel context [10]; Cheong  et. al. presents an authoring interactive narrative framework  to help users construct branching story structure [2].  Although rare of these projects incorporate digital media as  our goal, they provide insights of story analysis to our work.   ");
                namesAndText.Add("5", "CONCLUSION AND FUTURE WORK  We have presented Raconteur, a story editing system that  helps users think about material in a story by showing related  scenes or other stories with similar goals. We suggest that  presenting analogous stories can provide a helpful guideline  for users to tell their stories. Our formative user study shows  that this kind of analogy finding is particularly helpful in the  case where users have large libraries or complex stories.  Future work will focus on increasing relevance of  suggestions, improving interactivity of media selection and  output previews, and conducting detailed evaluation. We also  are exploring augmenting the media capture experience as  well as post-production editing. We aim for providing a fun  and productive environment for storytelling. Maybe it will  help your friends become more interested in watching your  vacation movies, after all.   ");
                namesAndText.Add("6", "REFERENCES  ");
                namesAndText.Add("6.1", "1. Barry, B.  and Davenport, G. Documenting Life:  Videography and Common Sense. In Proc. ICME2003:  the 2003 IEEE Intl. Conf. on Multimedia and Expo,  IEEE Press (2003), Baltimore, MD, USA.   ");
                namesAndText.Add("6.2", "2. Cheong, Y., Kim, Y. Min, W. and Shim, E. PRISM: A  Framework for Authoring Interactive Narratives. In Proc.  ICIDS 2008: the 1st Joint Intl. Conf. on Interactive Digital  Storytelling: Interactive Storytelling, ACM Press (2008),  Erfurt, German.  ");
                namesAndText.Add("6.3", "3. Cooper, M., Foote, J., Girgensohn, A., and Wilcox, L.  Temporal Event Clustering for Digital Photo Collections.  In Proc. MM’03: the 11th ACM Intl. Conf. on  Multimedia, ACM Press (2003), Berkeley, CA, USA.  ");
                namesAndText.Add("6.4", "4. Gentner, D. Analogy. In A Companion to Cognitive  Science, Oxford University Press (1998), pp. 107-113.  ");
                namesAndText.Add("6.5", "5. Joshi, D. and Luo, J. Inferring Generic Activities and  Events from Image Content and Bags of Geo-tags. In  Proc. CIVR’08: the 2008 Intl. Conf. on Content-based  image and video retrieval, ACM Press (2008), Niagara  Falls, Canada.  ");
                namesAndText.Add("6.6", "6. Kirk, D., Sellen, A., Rother, C., and Wood, K.  Understanding Photowork. In Proc. CHI2006:  the 24th  Intl. Conf. on Human factors in computing systems,  ACM Press (2006), Montréal, Québec, Canada.  ");
                namesAndText.Add("6.7", "7. Lebowitz, M. Story Telling as Planning and Learning.  Poetics 14 (1985), pp. 483-502.  ");
                namesAndText.Add("6.8", "8. Lieberman, H. and Liu, H. Adaptive Linking Between  Text and Photos using Common Sense Reasoning. In  Proc. AH2002: the 2nd Intl. Conf. on Adaptive  hypermedia and adaptive web-based systems, ACM  Press (2002), London, UK.  ");
                namesAndText.Add("6.9", "9. Liu, H. and Singh, P. ConceptNet: a Practical  Commonsense Reasoning Toolkit. In BT Technology  Journal (2004), 22, 4, 211-226.  ");
                namesAndText.Add("6.10", "10. Riedl, M. and León, C. Generating Story Analogues. In  Proc. AIIDE09: the 5th Conf. on Artificial Intelligence for  Interactive Digital Entertainment, AAAI Press (2009),  Palo Alto, CA, USA.  ");
                namesAndText.Add("6.11", "11. Schank, R. Tell Me a Story: A New Look at Real and  Artificial Intelligence. Northwestern University Press  (1991).  ");
                namesAndText.Add("6.12", "12. Shen, Y.-T., Lieberman, H., and Davenport G. What’s  Next? Emergent Storytelling from Video Collections. In  Proc. CHI2009:  the 27th Intl. Conf. on Human factors in  computing systems, ACM Press (2009), Boston, MA,  USA.  ");
                namesAndText.Add("6.13", "13. Speer, R., Havasi, C., and Lieberman, H. AnalogySpace:  Reducing the Dimensionality of Common Sense  Knowledge. In Proc. AAAI2008: the 23rd AAAI Conf. on  Artificial intelligence, AAAI Press (2008), Chicago, IL,  USA.  ");
                viewRangesOnDevice.Add("3.11cont", CreateViewRange(new Rectangle(4, 35, 15, 20), "rect"));
                viewRangesOnDevice.Add("4.1", CreateViewRange(new Rectangle(22, 35, 16, 20), "head"));
                viewRangesOnDevice.Add("4.2", CreateViewRange(new Rectangle(41, 35, 18, 20), "rect"));
                viewRangesOnDevice.Add("4.3", CreateViewRange(new Rectangle(62, 35, 16, 20), "rect"));
                viewRangesOnDevice.Add("5", CreateViewRange(new Rectangle(81, 35, 21, 20), "head"));
                viewRangesOnDevice.Add("6", CreateViewRange(new Rectangle(4, 5, 4, 20), "head"));
                viewRangesOnDevice.Add("6.1", CreateViewRange(new Rectangle(11, 5, 4, 20), "rect"));
                viewRangesOnDevice.Add("6.2", CreateViewRange(new Rectangle(18, 5, 5, 20), "rect"));
                viewRangesOnDevice.Add("6.3", CreateViewRange(new Rectangle(26, 5, 4, 20), "rect"));
                viewRangesOnDevice.Add("6.4", CreateViewRange(new Rectangle(33, 5, 3, 20), "rect"));
                viewRangesOnDevice.Add("6.5", CreateViewRange(new Rectangle(39, 5, 6, 20), "rect"));
                viewRangesOnDevice.Add("6.6", CreateViewRange(new Rectangle(48, 5, 5, 20), "rect"));
                viewRangesOnDevice.Add("6.7", CreateViewRange(new Rectangle(56, 5, 3, 20), "rect"));
                viewRangesOnDevice.Add("6.8", CreateViewRange(new Rectangle(62, 5, 5, 20), "rect"));
                viewRangesOnDevice.Add("6.9", CreateViewRange(new Rectangle(70, 5, 3, 20), "rect"));
                viewRangesOnDevice.Add("6.10", CreateViewRange(new Rectangle(76, 5, 4, 20), "rect"));
                viewRangesOnDevice.Add("6.11", CreateViewRange(new Rectangle(83, 5, 3, 20), "rect"));
                viewRangesOnDevice.Add("6.12", CreateViewRange(new Rectangle(89, 5, 5, 20), "rect"));
                viewRangesOnDevice.Add("6.13", CreateViewRange(new Rectangle(97, 5, 5, 20), "rect"));
            }

            #endregion

            #region Study Paper 1 - Teaching Objects-first

            // page 1 - Objects-first
            if (nameOfPaper == "paper2" && pageNum == 1)
            {
                namesAndText.Add("title", "Teaching Objects-first In Introductory Computer Science");
                namesAndText.Add("author1", "Stephen Cooper*  Computer Science Dept.  Saint Joseph's University  Philadelphia, PA 19131  scooper@sju.edu  ");
                namesAndText.Add("author2", "Wanda Dann*  Computer Science Dept.  Ithaca College  Ithaca, NY 14850  wpdann@ithaca.edu  ");
                namesAndText.Add("author3", "Randy Pausch  Computer Science Dept.  Carnegie Mellon University  Pittsburgh, PA 15213  pausch@cmu.edu ");
                namesAndText.Add("abstract", "Abstract  An objects-first strategy for teaching introductory computer  science courses is receiving increased attention from CS  educators.  In this paper, we discuss the challenge of the objects- first strategy and present a new approach that attempts to meet this  challenge.  The new approach is centered on the visualization of  objects and their behaviors using a 3D animation environment.  Statistical data as well as informal observations are summarized to  show evidence of student performance as a result of this approach.   A comparison is made of the pedagogical aspects of this new  approach with that of other relevant work.  ");
                namesAndText.Add("categories", "Categories and Subject Descriptors  K.3 [Computers & Education]: Computer & Information  Science Education – Computer Science Education.   ");
                namesAndText.Add("general", "General Terms  Documentation, Design, Human Factors,   ");
                namesAndText.Add("keywords", "Keywords  Visualization, Animation, 3D, Objects-First, Pedagogy, CS1  ");
                namesAndText.Add("introduction", "1 Introduction  The ACM Computing Curricula 2001 (CC2001) report [8]  summarized four approaches to teaching introductory computer  science and recognized that the “programming-first” approach is  the most widely used approach in North America. The report  describes three implementation strategies for achieving a  programming-first approach: imperative-first, functional-first, and  objects-first. While the first two strategies have been utilized for  quite some time, it is the objects-first strategy that is presently  attracting much interest. Objects-first “emphasizes the principles  of object-oriented programming and design from the very  beginning…. [The strategy] begins immediately with the notions  of objects and inheritance….[and] then goes on to introduce more  traditional control structures, but always in the context of an  overarching focus on object-oriented design” [8, Chapter 7].  ");
                namesAndText.Add("astrisk", "*This work was partially supported by NSF grant DUE-0126833");
                namesAndText.Add("copyright", "  Permission to make digital or hard copies of all or part of this work for  personal or classroom use is granted without fee provided that copies are  not made or distributed for profit or commercial advantage and that copies  bear this notice and the full citation on the first page. To copy otherwise,  or republish, to post on servers or to redistribute to lists, requires prior  specific permission and/or a fee.  SIGCSE’03 February 19-23, 2003, Reno, Nevada, USA.  Copyright 2003 ACM 1-58113-648-X/03/0002…$5.00.  ");
                namesAndText.Add("para2", "The Challenge of Objects-first: The authors of CC2001 admit  that an objects-first strategy adds complexity to teaching and  learning introductory programming. Why is this  so?  The  classic   instruction  methodology  for  an introduction to programming is  to start with simple programs and gradually advance to complex  programming examples and projects. The classic approach allows  a somewhat gentle learning curve, providing time for the learner to  assimilate and build knowledge incrementally. An objects-first  strategy is intended to have students work immediately with  objects. This means students must dive right into classes and  objects, their encapsulation (public and private data, etc.) and  methods (the constructors, accessors, modifiers, helpers, etc.). All  this is in addition to mastering the usual concepts of types,  variables, values, and references, as well as with the often- frustrating details of syntax. Now, add event-driven concepts to  support interactivity with GUIs! As argued by [11], learning to  program objects-first requires students grasp 'many different  concepts, ideas, and skills…almost concurrently. Each of these  skills presents a different mental challenge.'     ");
                namesAndText.Add("para3", "The additional complexity of an objects-first strategy is  understood when considered in terms of the essential concepts to  be mastered. The functional-first strategy initially focuses on  functions, deferring a discussion of state until later. The  imperative-first strategy initially focuses on state, deferring a  discussion of functions until later. The objects-first strategy  requires an initial discussion of both state and functions. The  challenge of an objects-first strategy is to provide a way to help  novice programmers master both of these concepts at once.   ");
                namesAndText.Add("para4", "2 Instructional Support Materials  In response to interest in an objects-first approach, several texts  and software tools have been published/developed that promote  this strategy (such as [1, 12]). Four recent software tools are  worthy of mention as using an objects-first approach: BlueJ [9],  Java Power Tools [11], Karel J. Robot [2], and various graphics  libraries. Interestingly, all these tools have a strong  visual/graphical component; to help the novice “see” what an  object actually is – to develop good intuitions about  objects/object-oriented programming.  ");
                namesAndText.Add("para5", "BlueJ [9] provides an integrated environment in which  the user generally starts with a previously defined set of classes.  The project structure is presented graphically, in UML-like  fashion. The user can create objects and invoke methods on those  objects to illustrate their behavior. Java Power Tools (JPT) [11]  provides a comprehensive, interactive GUI, consisting of several  classes with which the student will work. Students interact with  the GUI, and learn about the behaviors of the GUI classes through  this interaction. Karel J. Robot [2] uses a microworld with a robot  to help students learn about objects. As in Karel [10], Robots are \nContinued on the next page ");
                viewRangesOnDevice.Add("title", CreateViewRange(new Rectangle(5, 12, 5, 36), "rect"));            
                viewRangesOnDevice.Add("author1", CreateViewRange(new Rectangle(13, 45, 12, 10), "rect"));
                viewRangesOnDevice.Add("author2", CreateViewRange(new Rectangle(13, 25, 12, 10), "rect"));
                viewRangesOnDevice.Add("author3", CreateViewRange(new Rectangle(13, 5, 12, 10), "rect"));
                viewRangesOnDevice.Add("abstract", CreateViewRange(new Rectangle(28, 35, 15, 20), "head"));
                viewRangesOnDevice.Add("categories", CreateViewRange(new Rectangle(46, 35, 3, 20), "rect"));
                viewRangesOnDevice.Add("general", CreateViewRange(new Rectangle(51, 35, 3, 20), "rect"));
                viewRangesOnDevice.Add("keywords", CreateViewRange(new Rectangle(56, 35, 3, 20), "rect"));
                viewRangesOnDevice.Add("introduction", CreateViewRange(new Rectangle(62, 35, 23, 20), "head"));
                viewRangesOnDevice.Add("astrisk", CreateViewRange(new Rectangle(88, 35, 3, 20), "rect"));      
                viewRangesOnDevice.Add("copyright", CreateViewRange(new Rectangle(94, 35, 8, 20), "rect"));       
                viewRangesOnDevice.Add("para2", CreateViewRange(new Rectangle(28, 5, 26, 20), "head"));             
                viewRangesOnDevice.Add("para3", CreateViewRange(new Rectangle(57, 5, 15, 20), "rect"));            
                viewRangesOnDevice.Add("para4", CreateViewRange(new Rectangle(75, 5, 16, 20), "head"));             
                viewRangesOnDevice.Add("para5", CreateViewRange(new Rectangle(94, 5, 15, 20), "rect"));            
            }

            // page 2 - Objects-first
            if (nameOfPaper == "paper2" && pageNum == 2)
            {
                namesAndText.Add("2.2cont", "added to a 2-D grid. Methods may be invoked on the robots to  move and turn them, and to have the robots handle beepers. Bruce  et al. [3] and Roberts [13] use graphics libraries in an object-first  approach. Here, there is some sort of canvas onto which objects  (e.g. 2-D shapes) are drawn. These objects may have methods  invoked on them and they react accordingly.  ");
                namesAndText.Add("2.3", "In the remainder of this paper, we present a new tactic  and software support for an objects-first strategy. The software  support for this new approach is a 3D animation tool. 3D  animation assists in providing stronger object visualization and a  flexible, meaningful context for helping students to “see” object- oriented concepts. (A more detailed comparison of the above tools  with our approach is provided in a later section.)   ");
                namesAndText.Add("3.1", "3 Our Approach  Our motivation in researching and developing this new approach  is to meet the challenge of an objects-first approach. Our approach  meets the challenge by: • Reducing the complexity of details that the novice  programmer must overcome  • Providing a design first approach to objects  • Visualizing objects in a meaningful context In this approach, we use Alice, a 3D interactive, animation,  programming environment for building virtual worlds, designed  for novices. The Alice system, developed by a research group at  Carnegie Mellon under direction of one of the authors, is freely  available at www.alice.org. A brief description of the interface is  provided.   ");
                namesAndText.Add("fig1", "Figure 1. The Alice Interface");
                namesAndText.Add("3.2", "Alice provides an environment where students can use/modify 3D  objects and write programs to generate animations. A screen- capture of the interface is shown in Figure 1. The interface  displays an object tree (upper left) of the objects in the current  world, the initial scene (upper center), a list of events in this world  (upper right), and a code editor (lower right). The overlapping  window tabs in the lower left allow for querying of properties,  dragging instructions into the code editor, and the use of sound.    ");
                namesAndText.Add("3.3", "Student Programs: A student adds 3D objects to a small  virtual world and arranges the position of each object in the world.  Each object encapsulates its own data (its private properties such  as height, width, and location) and has its own member methods.  While it is beyond the scope of this paper to discuss all the details,\nContinues on next column");
                namesAndText.Add("3.3cont", "a brief example is discussed below to illustrate some of the  principles. Interested readers may wish to read [4, 6, 7] for a more  complete description. Figure 2 contains an initial scene that  includes a frog (named kermit), a beetle (ladybug), a flower  (redFlower), and several other objects around a pond.    ");
                namesAndText.Add("fig2", "Figure 2. An initial scene in an Alice world");
                namesAndText.Add("3.4", "Once the virtual world is initialized, the program code is created  using a drag-and-drop smart editor. Using the mouse, an object is  mouse-clicked and dragged into the editor where drop-down  menus allow the student to select from primitive methods that  send a message to the object. A student can write his/her own  user-defined methods and functions, and these are automatically  added to the drop-down menus.   ");
                namesAndText.Add("3.5", "In this example, the task is for kermit to hop over to the  ladybug. The code is illustrated in Figure 3. It is interesting to note  that the built-in predicates (“Questions” in Alice-lingo) “is at least  m meters away from n”, “is within x meters of y”, and “is in front  of z” all return spacial information about the objects in question.  (Users may define their own, user-defined, questions, at both the  world-level as well as at the character-level.) The bigHop(number  n) and littleHop() methods are both character-level. In other  words, the basic frog class has been extended to create a frog that  knows how to make a small hop and how to hop over a large  object (receiving a parameter as to how high it must hop).   ");
                namesAndText.Add("3.6", "This example illustrates some important aspects of our  approach. The mechanism for generating code relies on visual  formatting rather than details of punctuation. The gain from this  no-type editing mechanism is a reduction in complexity. Students  are able to focus on the concepts of objects and encapsulation,  rather than dealing with the frustration of parentheses, commas,  and semicolons. We hasten to note that program structure is still  part of the visual display and the semantics of instructions are still  learned. A switch is used to display Java-like punctuation to  support a later transition to C++/Java syntax.  ");
                namesAndText.Add("3.7", "Three-dimensionality provides a sense of reality for  objects. In the 3D world, students may write methods from scratch  to make objects perform animated tasks. The animation task  provides a meaningful context for understanding classes, objects,  methods, and events.  ");
                viewRangesOnDevice.Add("2.2cont", CreateViewRange(new Rectangle(4, 35, 6, 20), "rect"));
                viewRangesOnDevice.Add("2.3", CreateViewRange(new Rectangle(13, 35, 7, 20), "rect"));
                viewRangesOnDevice.Add("3.1", CreateViewRange(new Rectangle(23, 35, 17, 20), "head"));
                viewRangesOnDevice.Add("fig1", CreateViewRange(new Rectangle(44, 35, 26, 20), "img"));
                viewRangesOnDevice.Add("3.2", CreateViewRange(new Rectangle(73, 35, 16, 20), "rect"));
                viewRangesOnDevice.Add("3.3", CreateViewRange(new Rectangle(92, 35, 10, 20), "head"));
                viewRangesOnDevice.Add("3.3cont", CreateViewRange(new Rectangle(4, 5, 6, 20), "rect"));
                viewRangesOnDevice.Add("fig2", CreateViewRange(new Rectangle(13, 5, 26, 20), "img"));
                viewRangesOnDevice.Add("3.4", CreateViewRange(new Rectangle(42, 5, 9, 20), "rect"));
                viewRangesOnDevice.Add("3.5", CreateViewRange(new Rectangle(54, 5, 15, 20), "rect"));
                viewRangesOnDevice.Add("3.6", CreateViewRange(new Rectangle(72, 5, 16, 20), "rect"));
                viewRangesOnDevice.Add("3.7", CreateViewRange(new Rectangle(91, 5, 8, 20), "rect"));  
            }

            // page 3 - Objects-first
            if (nameOfPaper == "paper2" && pageNum == 3)
            {
                namesAndText.Add("fig3", "Figure 3. The code to have kermit hop over to the ladybug");
                namesAndText.Add("4.1", "4 Observations   We have been teaching and researching this new objects-first  approach in an introduction to programming course for the past 3  years. One of the authors uses this approach in a ½ semester  course that students take concurrently with CS1. Another author  uses this approach as part of a course that students take before  CS1. While early quantitative results are discussed in the next  section, we present more informal observations in this section.     ");
                namesAndText.Add("4.2", " Strengths: We have seen that students develop:  ");
                namesAndText.Add("4.3", "• A strong sense of design. In our approach, we use  storyboarding and pseudocode to develop designs. This may be  influenced by the nature of our open-ended assignments.  However, we see students in later classes writing down their  thoughts about an assignment on paper first, before going to the  computer.  ");
                namesAndText.Add("4.4", "• A contextualization for objects, classes, and object-oriented  programming. We believe that this is one of the big “wins” for  our approach. Everything in the student’s virtual world is an  object! Exercises and lab projects set up scenes where objects  fly, hop, swim, and interact in highly imaginative movie-like  simulations and games.   ");
                namesAndText.Add("4.5", "• An appreciation of trial and error. Students learn to 'try  out' individual animation instructions as well as their user- defined methods. Each animation instruction causes a visible  change in the animation.  Students learn to relate individual  instructions, and methods to the animated action on the screen  [7]. This direct relationship can be used to support development  of debugging skills.  ");
                namesAndText.Add("4.6", "• An incremental construction approach, both for character  (class)-level as well as world-level methods. Students do not  write the whole program first. They program incrementally,  one method at a time, testing out each piece.   ");
                namesAndText.Add("4.7", "• A firm sense of objects. The strong visual environment  helps here.  ");
                namesAndText.Add("4.8", "• Good intuitions concerning encapsulation.  Some state  information can be modified by invoking methods on an object.  For example, an object's position can be changed by invoking a  move method. But the actual spatial coordinates that represent  the object's position cannot be directly accessed.  ");
                namesAndText.Add("4.9", "• The concept of methods as a means of requesting an object  to do something. The way to make an object perform a task is  to send the object a message.  ");
                namesAndText.Add("4.10", "• A strong sense of inheritance, as students write code to  create more powerful classes.     ");
                namesAndText.Add("4.11", " • An ability to collaborate. Students work on building the  characters individually and then combine them to build virtual  worlds and animations in group projects.  ");
                namesAndText.Add("4.12", "• An understanding of Boolean types. Students are  prevented, by the smart-editor, from dragging incorrect data- type expressions into if statements and loops, for example.  ");
                namesAndText.Add("4.13", "• A sense of the program state. This is of particular  importance, as mentioned earlier in this paper. This topic is  discussed at length in [7].   ");
                namesAndText.Add("4.14", "• An intuitive sense of behaviors and event-driven  programming.  ");
                namesAndText.Add("4.15", "One other observation is that it is possible to have students  either create their programs from scratch or to build virtual worlds  with characters which already have many specialized methods pre- defined. This latter case allows students to experiment with  modifying existing classes/programs.  ");
                namesAndText.Add("4.16", "Weakness: A strength of our approach is also a source of  weakness. Students do not develop a detailed sense of syntax,  even with the C++/Java syntax switch turned on, as they only drag  the statements/expressions into the code window. They do not get  the opportunity to experience such errors as mismatched braces,  missing semicolons, etc. Our experience with students making the  transition from Alice to C++/ Java is that students quickly master  the syntax.   ");
                namesAndText.Add("5.1", "5 Results  Table 1 illustrates the results of students at Ithaca College and  Saint Joseph’s University who took a course using our proposed  approach during the 2001-2002 school year. The weakest 21 CS  majors (defined as those CS students who were not ready for  calculus and who had no previous programming experience) were  invited to take a course using our approach, either concurrent with,  or preliminary to CS1.  11 of the 21 students took the course,\nContinued on the next page");
                viewRangesOnDevice.Add("fig3", CreateViewRange(new Rectangle(4, 8, 21, 44), "img"));
                viewRangesOnDevice.Add("4.1", CreateViewRange(new Rectangle(28, 35, 11, 20), "head"));
                viewRangesOnDevice.Add("4.2", CreateViewRange(new Rectangle(42, 35, 4, 20), "head"));
                viewRangesOnDevice.Add("4.3", CreateViewRange(new Rectangle(49, 35, 7, 20), "rect"));
                viewRangesOnDevice.Add("4.4", CreateViewRange(new Rectangle(59, 35, 8, 20), "rect"));
                viewRangesOnDevice.Add("4.5", CreateViewRange(new Rectangle(70, 35, 10, 20), "rect"));
                viewRangesOnDevice.Add("4.6", CreateViewRange(new Rectangle(83, 35, 4, 20), "rect"));
                viewRangesOnDevice.Add("4.7", CreateViewRange(new Rectangle(90, 35, 3, 20), "rect"));
                viewRangesOnDevice.Add("4.8", CreateViewRange(new Rectangle(96, 35, 6, 20), "rect"));
                viewRangesOnDevice.Add("4.9", CreateViewRange(new Rectangle(28, 5, 4, 20), "rect"));
                viewRangesOnDevice.Add("4.10", CreateViewRange(new Rectangle(35, 5, 3, 20), "rect"));
                viewRangesOnDevice.Add("4.11", CreateViewRange(new Rectangle(41, 5, 4, 20), "rect"));
                viewRangesOnDevice.Add("4.12", CreateViewRange(new Rectangle(48, 5, 4, 20), "rect"));
                viewRangesOnDevice.Add("4.13", CreateViewRange(new Rectangle(55, 5, 4, 20), "rect"));
                viewRangesOnDevice.Add("4.14", CreateViewRange(new Rectangle(62, 5, 3, 20), "rect"));
                viewRangesOnDevice.Add("4.15", CreateViewRange(new Rectangle(68, 5, 6, 20), "rect"));
                viewRangesOnDevice.Add("4.16", CreateViewRange(new Rectangle(77, 5, 10, 20), "head"));
                viewRangesOnDevice.Add("5.1", CreateViewRange(new Rectangle(90, 5, 12, 20), "head"));
            }

            // page 4 - Objects-first
            if (nameOfPaper == "paper2" && pageNum == 4)
            {
                namesAndText.Add("5.1cont", "while 10 did not.  (Some students who did not take the course had  scheduling conflicts.)  ");
                namesAndText.Add("tab1", "Table 1: Students taking Alice, 2001-2002");
                namesAndText.Add("5.2", "  The results show that the 11 students who took the Alice-based  course did better in CS1 than the total group, and significantly  better than the 10 students who were of a similar background. Not  only did the control group perform better in CS1, the lower  variance indicates that a smaller percentage of those students  performed poorly in CS1. Perhaps the most telling statistic is the  percentage of students who continued on to CS2, the next  computer science class. 65% of all the students who took CS1  continued on to CS2. Of the students in the test group (who took  our course with Alice), 91% continued on to CS2. Only 10% of  the control group enrolled in CS2. A larger group of students is  being studied (in much more detail) this current (2002-2003)  academic year, as part of an NSF supported study.  ");
                namesAndText.Add("5.3", "The authors have a textbook (to be published by  Prentice-Hall for Fall 2003). An early draft is available at  www.ithaca.edu/wpdann/alice2002/alicebook.html The URL for  the solutions is available by contacting the authors. And, a set of  lecture notes and sample virtual worlds is available at:   http://www.sju.edu/~scooper/fall02csc1301/alice.html  ");
                namesAndText.Add("6.1", "  6 Comparison with other tools  In this section we explore what we consider to be our relative  strengths and weaknesses as compared to other object-first tools  mentioned earlier. It is important to note that, as we have not seen  studies detailing actual effectiveness of many of the other tools,  we are hesitant to state too strongly the degree to which we think  such tools do or do not work.  ");
                namesAndText.Add("6.2", "Events: JPT makes heavy use of GUIs, and both JPT and Bruce’s  ObjectDraw library rely on event-driven programming. Kölling  and Rosenberg [9] state that building GUIs is “very time  intensive”, and argue that the GUI code is an “example that has  very idiosyncratic characteristics that are not common to OO in  general.” Culwin [5] argues “the design of an effective GUI  requires a wider range of skills than those of software  implementation…. Even if an optimal interface is not sought at  this stage it must be emphasized to students…that there is much  more to the construction of a GUI than the collecting together of a  few widgets and placing it in front of the user.” While we might  not go as far as these criticisms, it is clear that event handling does  add a layer of complexity. In our approach, the use of events is  optional and is accomplished through the use of several powerful  primitives. This makes the presentation of events and event  handling quite simple. We disagree with the statement “it is not  possible to do Objects-first” without also doing GUI First!”[11],  as both our approach and some of the graphics libraries do  accomplish an object-first approach without the use of a GUI  (though adding events generally makes virtual worlds much more  fun for the students).  ");
                namesAndText.Add("6.3", "Modifying existing code: BlueJ and JPT depend on starting  with programs that consist of previously written code. Bruce is  concerned “these approaches will leave students feeling they have  no understanding of how to write complete programs.” The BlueJ  and JPT authors maintain that, due to complexity of object- oriented design, it is favorable for novices to start with  partially/completely developed projects and to modify them. Our  approach allows the instructor to choose to use partially developed  programs in introductory worlds. But, we generally have students  build virtual worlds from scratch.   ");
                namesAndText.Add("6.4", "Use of the tool throughout the CS1 course: Each of these  tools, with the exception of Karel J. Robot, is (or at least seems to  be) capable of being used throughout the CS1 course. We have  designed lecture materials to be used as an initial introduction to  object-oriented programming, occupying the first 3-6 weeks of a  CS1 course. It would be possible to intersperse the teaching of  Alice with the teaching of, say, Java, throughout the semester.    ");
                namesAndText.Add("6.5", "Complexity of syntax: The use of graphics libraries is likely  the most complex approach. Even though libraries are provided,  students still must write Java/C++ programs from scratch,  mastering a non-trivial amount of syntax (regardless whether they  understand the semantics of what they are writing). Then they  need to understand the particulars of the graphics library. Karel J.  Robot has a fair bit of Java that needs to be mastered before being  able to write a program. The BlueJ and JPT approaches are  somewhat simpler, as students only modify existing code. Yet, it  is still necessary to write correct Java code, and certain errors  (such as missing brackets or trying to place code in the wrong  location, or invoking a method with a bad parameter) can lead to  errors in the code provided to the student -- and the student may  not know how to start debugging code that he/she did not write. ");
                namesAndText.Add("6.6", " Concurrency: As Culwin writes [5], “if an early introduction of  GUIs is advocated within an object first approach, the importance  of concurrency cannot be avoided.” Alice supports concurrency,  providing primitives for performing actions simultaneously.   ");
                namesAndText.Add("6.7", "Examples: This is a challenge for all objects-first approaches.  Developing a large collection of examples (whether to be used as  instructional aids, assignments or exam questions) is a time- consuming task that must be solved if these tools, together with  their associated approach are to be successful. One product of our  research efforts is a resource of examples, exercises, and projects  with solutions.  It does need to be made larger, which we are doing  each semester.    ");
                namesAndText.Add("7.1", "7 Conclusions  The authors strongly believe that, as long as object-oriented  languages are the popular language of choice in CS1, the objects- first approach is the best way to help students master the  complexities of object-oriented programming. We believe that  other tools mentioned here are quite useful in teaching objects- first. (We have used most of them ourselves.)  We have been  particularly impressed with the results we have seen so far with  the approach we have presented here – we have been able to  significantly reduce the attrition of our most at-risk majors. The  current NSF study will examine the effectiveness of our proposed  approach in greater detail, and with larger numbers of students.  Additionally, we hope to gain feedback from some of the  additional institutions that are using our materials and our  approach.   ");
                viewRangesOnDevice.Add("5.1cont", CreateViewRange(new Rectangle(4, 35, 3, 20), "rect"));
                viewRangesOnDevice.Add("tab1", CreateViewRange(new Rectangle(10, 38, 16, 14), "tab"));
                viewRangesOnDevice.Add("5.2", CreateViewRange(new Rectangle(29, 35, 18, 20), "rect"));
                viewRangesOnDevice.Add("5.3", CreateViewRange(new Rectangle(50, 35, 6, 20), "rect"));
                viewRangesOnDevice.Add("6.1", CreateViewRange(new Rectangle(59, 35, 10, 20), "head"));
                viewRangesOnDevice.Add("6.2", CreateViewRange(new Rectangle(72, 35, 30, 20), "head"));
                viewRangesOnDevice.Add("6.3", CreateViewRange(new Rectangle(4, 5, 18, 20), "head"));
                viewRangesOnDevice.Add("6.4", CreateViewRange(new Rectangle(25, 5, 9, 20), "head"));
                viewRangesOnDevice.Add("6.5", CreateViewRange(new Rectangle(37, 5, 18, 20), "head"));
                viewRangesOnDevice.Add("6.6", CreateViewRange(new Rectangle(58, 5, 4, 20), "head"));
                viewRangesOnDevice.Add("6.7", CreateViewRange(new Rectangle(65, 5, 10, 20), "head"));
                viewRangesOnDevice.Add("7.1", CreateViewRange(new Rectangle(78, 5, 24, 20), "head"));
            }

            // page 5 - Objects-first
            if (nameOfPaper == "paper2" && pageNum == 5)
            {
                namesAndText.Add("ref", "References  ");
                namesAndText.Add("1", "[1]  Arnow, D. and Weiss, G. Introduction to programming  using Java: an object-oriented approach, Java 2 update.  Addison-Wesley, 2001. ");
                namesAndText.Add("2", "[2]  Bergin, J., Stehlik, M., Roberts, J., and Pattis, R.  Karel  J. Robot a gentle introduction to the  art  of  object  oriented programming   in  Java. Unpublished  manuscript, available [August 31, 2002] from:  http://csis.pace.edu/~bergin/KarelJava2ed/Karel++Java Edition.html ");
                namesAndText.Add("3", "[3]  Bruce, K., Danyluk, A., & Murtagh, T. A library to  support a graphics-based object-first approach to CS 1.  In Proceedings of the 32nd SIGCSE technical  symposium on Computer Science Education (Charlotte,  North Carolina, February, 2001), 6-10. ");
                namesAndText.Add("4", "[4]  Cooper, S., Dann, W., & Pausch, R. Using animated 3d  graphics to prepare novices for CS1. Computer Science  Education Journal, to appear. ");
                namesAndText.Add("5", "[5]  Culwin, F. Object imperatives! In Proceedings of the  30th SIGCSE technical symposium on Computer  Science Education (New Orleans, Louisiana, March,  1999), 31-36. ");
                namesAndText.Add("6", "[6]  Dann, W., Cooper, S., & Pausch, R. Using visualization  to teach novices recursion. In Proceedings of the 6th  annual conference on Innovation and Technology in  Computer Science Education (Canterbury, England,  June, 2001), 109-112. ");
                namesAndText.Add("7", "[7]  Dann, W., Cooper, S., & Pausch, R. Making the  connection: programming with animated small worlds.  In Proceedings of the 5th annual conference on  Innovation and Technology in Computer Science  Education (Helsinki, Finland, July, 2000), 41-44.  ");
                namesAndText.Add("8", "[8]   Joint Task Force on Computing Curricula. Computing  Curricula 2001 Computer Science. Journal of  Educational Resources in Computing (JERIC), 1 (3es),  Fall 2001. ");
                namesAndText.Add("9", "[9]  Kölling, M. & Rosenberg, J., Guidelines for teaching  object orientation with Java. In Proceedings of the 6th  annual conference on Innovation and Technology in  Computer Science Education (Canterbury, England,  June, 2001), 33-36. ");
                namesAndText.Add("10", "[10] Pattis, R., Roberts, J, & Stehlik, M. Karel the robot: a  gentle introduction to the art of programming, 2nd  Edition. John Wiley & Sons, 1994. ");
                namesAndText.Add("11", "[11] Proulx, V., Raab, R., & Rasala, R. Objects from the  beginning – with GUIs. In Proceedings of the 7th  annual conference on Innovation and Technology in  Computer Science Education (Århus, Denmark, June,  2002), 65-69. ");
                namesAndText.Add("12", "[12] Riley, D. The object of Java: Bluej edition. Addison- Wesley, 2002. ");
                namesAndText.Add("13", "[13] Roberts, E. & Picard, A. Designing a Java graphics  library for CS1. In Proceedings of the 3rd annual  conference on Innovation and Technology in Computer  Science Education (Dublin, Ireland, July, 1998), 213- 218. ");
                viewRangesOnDevice.Add("ref", CreateViewRange(new Rectangle(4, 35, 4, 20), "head"));
                viewRangesOnDevice.Add("1", CreateViewRange(new Rectangle(11, 35, 4, 20), "rect"));
                viewRangesOnDevice.Add("2", CreateViewRange(new Rectangle(18, 35, 7, 20), "rect"));
                viewRangesOnDevice.Add("3", CreateViewRange(new Rectangle(28, 35, 6, 20), "rect"));
                viewRangesOnDevice.Add("4", CreateViewRange(new Rectangle(37, 35, 4, 20), "rect"));
                viewRangesOnDevice.Add("5", CreateViewRange(new Rectangle(44, 35, 5, 20), "rect"));
                viewRangesOnDevice.Add("6", CreateViewRange(new Rectangle(52, 35, 6, 20), "rect"));
                viewRangesOnDevice.Add("7", CreateViewRange(new Rectangle(61, 35, 6, 20), "rect"));
                viewRangesOnDevice.Add("8", CreateViewRange(new Rectangle(11, 5, 5, 20), "rect"));
                viewRangesOnDevice.Add("9", CreateViewRange(new Rectangle(19, 5, 6, 20), "rect"));
                viewRangesOnDevice.Add("10", CreateViewRange(new Rectangle(28, 5, 4, 20), "rect"));
                viewRangesOnDevice.Add("11", CreateViewRange(new Rectangle(35, 5, 6, 20), "rect"));
                viewRangesOnDevice.Add("12", CreateViewRange(new Rectangle(44, 5, 3, 20), "rect"));
                viewRangesOnDevice.Add("13", CreateViewRange(new Rectangle(50, 5, 6, 20), "rect"));
            }

            #endregion

            #region Study Paper 2 - Scalable Game Design

            // page 1 - Game Design
            if (nameOfPaper == "paper3" && pageNum == 1)
            {
                namesAndText.Add("title", "Using Scalable Game Design to Teach Computer Science  From Middle School to Graduate School  ");
                namesAndText.Add("author1", "Ashok R. Basawapatna  University of Colorado Boulder  Department of Computer Science  Boulder, CO. 80303  (720) 838-5838, 001   basawapa@colorado.edu  ");
                namesAndText.Add("author2", "Kyu Han Koh  University of Colorado Boulder  Department of Computer Science  Boulder, CO. 80303  (303) 495-0357, 001   kohkh@colorado.edu  ");
                namesAndText.Add("author3", "Alexander Repenning  University of Colorado Boulder  Department of Computer Science  Boulder, CO. 80303  (303) 492-1349, 001   ralex@cs.colorado.edu  ");
                namesAndText.Add("abstract", "ABSTRACT     A variety of approaches exist to teach computer science concepts  to students from K-12 to graduate school. One such approach  involves using the mass appeal of game design and creation to  introduce students to programming and computational thinking.  Specifically, Scalable Game Design enables students with varying  levels of expertise to learn important concepts relative to their  experience. This paper presents our observations using Scalable  Game Design over multiple years to teach middle school students,  college level students, graduate students, and even middle school  teachers fundamental to complex computer science and education  concepts. Results indicate that Scalable Game Design appeals  broadly to students, regardless of background, and is a powerful  teaching tool in getting students of all ages exposed and interested  in computer science. Furthermore, it is observed that many  student projects exhibit transfer enabling their games to explain  complex ideas, from all disciplines, to the general public.    ");
                namesAndText.Add("categories", "Categories and Subject Descriptors     K.3.2 Computer and Information Science Education  ");
                namesAndText.Add("general", "General Terms     Algorithms, Design, Experimentation, Human Factors.  ");
                namesAndText.Add("keywords", "Keywords     University Programming Education, Middle School  Programming, Scalable Game Design, Computational Thinking  Pattern, Transfer, Student Observation.  ");
                namesAndText.Add("1.1", "1. INTRODUCTION     Since the early 1990’s there have been multiple efforts to use end- user video game creation in an effort to teach programming.  Examples of these game creation tools include Alice, Scratch, and  AgentSheets among others [1,2,3]. The inherent appeal of video\nContinues on next column");
                namesAndText.Add("copyright", "Permission to make digital or hard copies of all or part of this work for  personal or classroom use is granted without fee provided that copies are  not made or distributed for profit or commercial advantage and that  copies bear this notice and the full citation on the first page. To copy  otherwise, or republish, to post on servers or to redistribute to lists,  requires prior specific permission and/or a fee.  ITiCSE’ 10, June 26–30, 2010, Bilkent, Ankara, Turkey.  Copyright 2010 ACM 978-1-60558-820-9/10/06…$10.00.  ");
                namesAndText.Add("1.1cont", "games to students gives teachers an entertaining way to introduce  the otherwise technical practice of programming [4,5]. These tools  have multiple advantages over conventional programming  languages such as C++ or Java [2]. For example, to create a  simple game, complete with graphics, in C++ or Java can take  weeks, or even months of learning. In contrast, as we have  observed implementing AgentSheets in numerous classes at  different levels ranging from middle school to graduate school,  students with no prior programming experience can create their  first game in 5 hours. Though game creation is rapid, students are  able to learn everything from low level if/then conditionality  statements to important programming computational thinking  patterns, object-oriented design, and high level concepts like  designing games for educational purposes [6]. This ability of  game design to scale up as students learn more is called Scalable  Game Design.  ");
                namesAndText.Add("1.2", "Scalable Game Design is important in educational applications  because it enables learning among both inexperienced and  experienced students [7]. A Scalable Game Design method of  teaching allows students to learn simple to complex concepts as  they gain expertise. This makes Scalable Game Design applicable  to the greatest number of students regardless of prior technical  knowledge, programming fluency, exposure to technology, or any  other background.    ");
                namesAndText.Add("1.3", "This paper introduces our research findings based on experiences  related to using game creation as a means of computer science  education. These experiences include work with middle school  students, university students, and middle school teachers creating  a diverse set of games from classic arcade games such as Frogger  to educational ecological simulations.  ");
                namesAndText.Add("2.1", "2. METHODOLOGY      2.1 AgentSheets     AgentSheets is a rapid visual agent-based game authoring  environment [8]. AgentSheets allows end-users to easily create  games and simulations providing abstraction at a sufficiently high  level so that novice programmers need not be concerned with low- level implementation details. In addition to having a low starting  threshold, AgentSheets also has a high ceiling allowing for  complex interactions between agents, such as sophisticated AI  techniques [3]. AgentSheets games can be exported as Java  applets, allowing for games to be emailed or embedded into a  student’s webpage. In most classes, students upload their games to  the Scalable Game Design Arcade (SGDA).  ");
                viewRangesOnDevice.Add("title", CreateViewRange(new Rectangle(10, 5, 6, 50), "rect"));
                viewRangesOnDevice.Add("author1", CreateViewRange(new Rectangle(19, 45, 13, 10), "rect"));
                viewRangesOnDevice.Add("author2", CreateViewRange(new Rectangle(19, 25, 13, 10), "rect"));
                viewRangesOnDevice.Add("author3", CreateViewRange(new Rectangle(19, 5, 13, 10), "rect"));
                viewRangesOnDevice.Add("abstract", CreateViewRange(new Rectangle(35, 35, 23, 20), "rect"));
                viewRangesOnDevice.Add("categories", CreateViewRange(new Rectangle(61, 35, 3, 20), "rect"));
                viewRangesOnDevice.Add("general", CreateViewRange(new Rectangle(66, 35, 3, 20), "rect"));
                viewRangesOnDevice.Add("keywords", CreateViewRange(new Rectangle(71, 35, 5, 20), "rect"));
                viewRangesOnDevice.Add("1.1", CreateViewRange(new Rectangle(79, 35, 11, 20), "head"));
                viewRangesOnDevice.Add("copyright", CreateViewRange(new Rectangle(93, 35, 8, 20), "rect"));
                viewRangesOnDevice.Add("1.1cont", CreateViewRange(new Rectangle(35, 5, 20, 20), "rect"));
                viewRangesOnDevice.Add("1.2", CreateViewRange(new Rectangle(58, 5, 10, 20), "rect"));
                viewRangesOnDevice.Add("1.3", CreateViewRange(new Rectangle(71, 5, 8, 20), "rect"));
                viewRangesOnDevice.Add("2.1", CreateViewRange(new Rectangle(82, 5, 20, 20), "head"));   
            }

            // page 2 - Game Design
            if (nameOfPaper == "paper3" && pageNum == 2)
            {
                namesAndText.Add("2.2", "2.2 Scalable Game Design Arcade  The Scalable Game Design Arcade is an open classroom  cyberlearning infrastructure that allows students to play,  download, and comment on and star rate fellow classmates’  games, as well as upload their own. At the university level,  students are asked to submit their assignments early to garner  feedback from other students before the final due date. Students  are also encouraged to play classmates’ games. An emerging  benefit of creating video games to teach programming is that since  many students inherently like playing games, they naturally play,  give feedback, and take ideas from fellow classmates’ games.  This is shown in questionnaire data, administered to the  University of Colorado “Educational Game Design” class,  wherein 22 students were surveyed; 85% of students said they  played 2 or more classmates’ games each week and 90% said they  played one or more [7].  ");
                namesAndText.Add("2.3.1", "2.3 Computational Thinking Patterns  Computational thinking patterns are agent interactions that not  only show up in other programming contexts, but also, show up in  other disciplines such as scientific and mathematical contexts. For  instance, the Diffusion pattern shows up in many different  scientific examples such as heat distribution problems in physics  as well as osmosis in biology and chemistry. Computational  thinking patterns are fundamental to the idea of transfer--  essentially, if a student wants to make a science simulation one or  many of these patterns are often employed [9]. We are currently  exploring a method of automatic recognition of computational  thinking patterns to detect transfer, however, our investigation so  far has focused on motivation.    ");
                namesAndText.Add("fig1", "Figure 1 Computational Thinking Pattern spiral and examples of how computational thinking concepts relate to other fundamental concepts in science. The patterns get more complex as one travels away from the center");
                namesAndText.Add("2.3.1cont", "Figure 1 depicts various computational thinking patterns and how  they might transfer to other disciplines. Students learn  computational science and computational thinking through  making their own games which consist of computational thinking  patterns [9,10]. The following computational thinking patterns are  used in making games such as Frogger, Sokoban, Centipede,\nContinued on next column  ");
                namesAndText.Add("2.3.1cont2", "Space Invaders and/or Sims. Table 1 lists these games and their  corresponding computational thinking patterns.   ");
                namesAndText.Add("tab1", "Table 1 Games and their corresponding computational thinking patterns");
                namesAndText.Add("2.3.2", "Example patterns include:  ");
                namesAndText.Add("2.3.3", "Generation: To satisfy this pattern, an agent is required to  generate a flow of other agents; for example, a bullet leaving a  gun (ie: the gun agent generates a flow of bullets) or a car  appearing from a tunnel are both examples of generation. In  predator/prey models generation is used to create offspring.  ");
                namesAndText.Add("2.3.4", "Absorption: This is the opposite pattern of generation. Instead of  an agent generating other agents, an agent absorbs a flow of other  agents in the absorption pattern (i.e. a tunnel absorbing cars). For  example, absorption is used to program a predator eating its prey.  ");
                namesAndText.Add("2.3.5", "Collision: This pattern represents the situation when two agents  physically collide. For example, a bullet or a missile hitting a  target creates a collision situation wherein the agents must react to  being collided with. In the game Frogger, for example, if a truck  collides with a frog, the frog must be 'squished.'  ");
                namesAndText.Add("2.3.6", "Transportation: Transportation represents the situation when one  agent carries another agents. For example, a turtle in Frogger  carries the frog as it crosses the river. In ecological simulations,  transportation can be used to have bees carry pollen for example.  ");
                namesAndText.Add("2.3.7", "Push: Push pattern is the pattern we see in the game of Sokoban.  A player in Sokoban is supposed to push boxes to cover targets.  As the player pushes the box in Sokoban, the box moves towards  the direction (up, down, right or left) it is pushed.   ");
                namesAndText.Add("2.3.8", "Pull: This pattern is the opposite pattern of push. An agent can  pull another adjunct agent or any number of agents serially  connected to the puller. For example, you can imagine that a  locomotive pulls a large number of railroad cars.   ");
                namesAndText.Add("2.3.9", "Diffusion: You can diffuse a certain value of an agent through  neighboring agents with a diffusion pattern. For example, a torch  agent can diffuse the value of heat through neighboring floor tile  agents. The closest eight neighboring floor tile agents to the torch  agent will have the highest value of heat, and tile agents that are  further away from the torch agent will have a lower heat value.   ");
                namesAndText.Add("2.3.10", "Hill Climbing: Hill climbing is a searching algorithm in computer  science. A hill climbing agent will look at neighboring values and  move toward the one with the largest value. Hill climbing can be  found in the game of Sims or Pacman. In the game of Pacman,  Ghosts chase Pacman by following the highest value of Pacman’s  scent that is diffused throughout the level. This is depicted in  Figure 2. As with the torch above, the floor tiles around where  Pacman is currently situated have the greatest scent value.   ");
                viewRangesOnDevice.Add("2.2", CreateViewRange(new Rectangle(4, 35, 27, 20), "head"));
                viewRangesOnDevice.Add("2.3.1", CreateViewRange(new Rectangle(34, 35, 24, 20), "head"));
                viewRangesOnDevice.Add("fig1", CreateViewRange(new Rectangle(61, 35, 29, 20), "img"));
                viewRangesOnDevice.Add("2.3.1cont", CreateViewRange(new Rectangle(93, 35, 9, 20), "rect"));
                viewRangesOnDevice.Add("2.3.1cont2", CreateViewRange(new Rectangle(4, 5, 3, 20), "rect"));
                viewRangesOnDevice.Add("tab1", CreateViewRange(new Rectangle(10, 5, 21, 20), "tab"));
                viewRangesOnDevice.Add("2.3.2", CreateViewRange(new Rectangle(34, 5, 3, 20), "rect"));
                viewRangesOnDevice.Add("2.3.3", CreateViewRange(new Rectangle(40, 5, 5, 20), "head"));
                viewRangesOnDevice.Add("2.3.4", CreateViewRange(new Rectangle(48, 5, 4, 20), "head"));
                viewRangesOnDevice.Add("2.3.5", CreateViewRange(new Rectangle(55, 5, 5, 20), "head"));
                viewRangesOnDevice.Add("2.3.6", CreateViewRange(new Rectangle(63, 5, 4, 20), "head"));
                viewRangesOnDevice.Add("2.3.7", CreateViewRange(new Rectangle(70, 5, 4, 20), "head"));
                viewRangesOnDevice.Add("2.3.8", CreateViewRange(new Rectangle(77, 5, 4, 20), "head"));
                viewRangesOnDevice.Add("2.3.9", CreateViewRange(new Rectangle(84, 5, 6, 20), "head"));
                viewRangesOnDevice.Add("2.3.10", CreateViewRange(new Rectangle(93, 5, 9, 20), "head"));
            }

            // page 3 - Game Design
            if (nameOfPaper == "paper3" && pageNum == 3)
            {
                namesAndText.Add("fig2", "Figure 2 Ghosts use hill climbing on Pacman's diffused scent  (pictured around Pacman) to track down Pacman  ");
                namesAndText.Add("3.1.1", "3. FIELD EXPERIENCE  3.1 Field Observations with Middle School  Students  The iDREAMS project at the University of Colorado Boulder is  funded by the National Science Foundation (NSF) to bring  Information Technology literacy into middle school curriculums.  Middle school computer education teachers from schools in tech- hub, urban, rural and remote areas of Colorado have been trained  to teach a one to two week AgentSheets course wherein students  make the classic arcade game Frogger [9]. Students are taught  numerous introductory programming concepts such as if/then  conditionality, Boolean logic, procedural abstraction and message  passing between agents. Students are also introduced to the notion  of computational thinking patterns.  ");
                namesAndText.Add("3.1.2", "Some of these middle school experiences come from the eCSite  project (Engaging Computer Science in Traditional Education),  also funded by the NSF through the GK-12 program. This  program gives graduate students in science and technology the  opportunity to share their research with students in middle and  high school classrooms.   ");
                namesAndText.Add("3.1.3", "From our personal experience in middle school classes and  computer clubs we have noticed various significant factors related  to teaching strategies when implementing game creation curricula.  In previous research we found that games enable the emerging  benefit of peer-to-peer interaction in the middle school  environment [7]. Students, especially in middle school computer  clubs, will wander around the classroom, play other student’s  games, ask about their game, give feedback to their classmates,  and then go back to their computer and implement what they have\nContinued on next column");
                namesAndText.Add("3.1.3cont", "just learned. Since video games are entertainment, viral peer  learning takes place in classrooms with little or no external input.  ");
                namesAndText.Add("3.1.4", "In our observations, adopting the creation of video games  increases class engagement. Students in a couple of schools asked  their teachers for extra game creation assignments which,  according to the teachers, is quite unusual for a middle school  computer science class. One of the most important observations  we noticed is that game programming, and specifically this game  programming module, is well liked by middle school students  regardless of gender and native language. This observation is  furthered by Table 2, which shows the initial findings of a post  survey given to middles school students after their game design  unit was completed. This table indicates that an overwhelming  majority of students regardless of language and gender enjoyed  the game programming class. Furthermore, almost all students  said that they would take another game design class. These results  indicate the benefits of employing video game design at the  middle school level as it uniformly increases all students’ interest  in computer science.   ");
                namesAndText.Add("3.1.5", "When teaching game programming to students in middle schools  we have found a marked difference as to how students treat  learning materials. Initially, in addition to brief teacher  instruction, a step by step in-depth tutorial was given to each  student to guide them through the game programming process.  This tutorial was long and had a huge amount of text. Not  surprisingly, students tended not to read the tutorial, even when  they needed help, opting instead to ask for the teacher or a  neighbor to help them.   ");
                namesAndText.Add("3.1.6", "In order to make game instruction more efficient, we developed a  new method of teaching materials entitled “cheat sheets.” Cheat  sheets for each day are about a page long and give students the  bare minimum amount of knowledge they need to accomplish a  given step in developing a game. More importantly, cheat sheets  at the most have 3 sentences of text, instead relying more on  pictures to describe the higher level programming tasks that must  be accomplished as well as specific images as to how to  accomplish them. Cheat sheets lend themselves extremely well to  video game learning due to the fact that video games are visual in  nature. Unlike other more conventional programming lessons,  video game projects can be described extremely well using  nothing but pictures. In our experience it was found that this cheat  sheet strategy was much more effective in getting students to use  the supplementary material.  ");
                namesAndText.Add("tab2", "Table 2 Game design with AgentSheets software can be motivational: preliminary statistics from use in middle school shows  that student motivation is high regardless gender and native language.    ");
                viewRangesOnDevice.Add("fig2", CreateViewRange(new Rectangle(4, 35, 20, 20), "img"));
                viewRangesOnDevice.Add("3.1.1", CreateViewRange(new Rectangle(27, 35, 21, 20), "head"));
                viewRangesOnDevice.Add("3.1.2", CreateViewRange(new Rectangle(51, 35, 7, 20), "rect"));
                viewRangesOnDevice.Add("3.1.3", CreateViewRange(new Rectangle(61, 35, 16, 20), "rect"));
                viewRangesOnDevice.Add("3.1.3cont", CreateViewRange(new Rectangle(4, 5, 3, 20), "rect"));
                viewRangesOnDevice.Add("3.1.4", CreateViewRange(new Rectangle(10, 5, 24, 20), "rect"));
                viewRangesOnDevice.Add("3.1.5", CreateViewRange(new Rectangle(37, 5, 14, 20), "rect"));
                viewRangesOnDevice.Add("3.1.6", CreateViewRange(new Rectangle(54, 5, 23, 20), "rect"));
                viewRangesOnDevice.Add("tab2", CreateViewRange(new Rectangle(80, 5, 22, 50), "tab"));
            }

            // page 4 - Game Design
           if (nameOfPaper == "paper3" && pageNum == 4)
           {
               namesAndText.Add("3.2.1", "3.2 Field Experience with University  Students: Educational Game Design Class  The Educational Game Design Class taught at the University of  Colorado Boulder is markedly different from the middle school  class. This class lasts for a whole semester, as opposed to 1-2  weeks, and includes a diverse group of upper division computer  science, engineering, education, media multidisciplinary, and  psychology undergraduate students as well as graduate students.  The majority of the students have typically been exposed to  programming before, though this is not a pre-requisite. This  allows all of the sophisticated computational thinking patterns in  Figure 1 to be taught and allows the class to emphasize the theory  of effective video game design as opposed to just programming  [6,11]. Since the class lasts an entire semester, the Scalable Game  Design Arcade becomes an integral part of instruction enabling  peer-to-peer learning among students [7].   ");
               namesAndText.Add("3.2.2", "The class is structured with 9 different gamelet programming  assignments. The first four are the same for everyone in the class:  Frogger, Sokoban (a box-target moving puzzle game), Centipede,  and a ‘Sims’ type simulation game. Since these assignments are  same for everyone, students benefit from downloading a fellow  classmate’s assignment submission from the Scalable Game  Design Arcade, if they do not understand a given topic.  ");
               namesAndText.Add("3.2.3", "The next four game assignments are individual student  assignments. This stage of the class is called “gamelet madness”  and students are asked to make one educational gamelet each  week. At this point students begin to apply the theory behind  creating educational video games and use the computational  thinking patterns they have learned. In addition to programming a  game, as part of the assignment, they are asked to evaluate one  another on how engaging and educational their fellow classmates’  games are.  ");
               namesAndText.Add("3.2.4", "At the end of gamelet madness students start working on their  final projects. Final projects can be any type of educational game  that the student deems would be effective in teaching a concept.  As they program their game, students have middle school students  ‘play-test’ their games at nearby after school computer clubs.  Play-testing allows students to garner critical feedback on their  final project and to see how engaging and educational the game  actually is to one of their target demographics.   ");
               namesAndText.Add("3.2.5", "In our observations, students learned much more than the above  computational thinking patterns in the Educational Game Design  Class. As mentioned above, the students in the class have diverse  backgrounds and skills, and often, the games they create try to  explain complex phenomena from their specific major to other  students and the general public. The emphasis on engagement and  educational value of games, as well as play testing with middle  school students, highlights the importance creating lessons that  are effective. Students exhibited an essential component of  computational thinking by combining computational thinking  patterns to create informative real-world learning simulations  often inspired by their respective areas of study. These projects  ranged from games that taught language to math puzzles, ecology  simulations, and even human body visualizations and DNA  sequencing simulations.  ");
               namesAndText.Add("3.2.6", "Through game creation, learning key points of educational game  theory, and play testing games with target audiences, students  could better understand what was effective and what was not in  terms of educational game creation. In play testing sessions and  final presentations we observed that many students had important\nContinued on next column");
               namesAndText.Add("3.2.7", "insights regarding the creation of educational materials for student  consumption.  ");
               namesAndText.Add("3.3.1", "3.3 Field Experience with Community College  Students and Middle School Teachers:  iDREAMS Summer Institute  In the summer of 2009, as part of the iDREAMS project, the  University of Colorado Boulder held a 2 week Summer Institute.  The goal of this Summer Institute was to enable middle school  teachers and community college students the ability to teach  computer science through game design in middle school  classrooms across the state of Colorado and even in South Dakota  Native American Reservations. In the first week of the Institute,  12 community college students from across the state of Colorado  participated in an intense 8-hour a day program wherein they  created Frogger, Space Invaders, a Diffusion Simulation, an  Ecosystem simulation and a Final Project. During the second  week, 21 middle school teachers, aided by community college  students from their respective geographical areas, were taught  how to create and teach AgentSheets units in their classes.  Teachers created the games Frogger, Sokoban, Pacman, and an  optional Final Project. The goal of the Institute is that the teachers  will go back to their middle schools and teach weeklong  AgentSheets units in the coming year with the help of a  community college student. So far this school year 8 districts have  taught a weeklong AgentSheets unit with over 1,100 middle  school students creating AgentSheets games. Preliminary results  from this implementation are shown in Table 2.  ");
               namesAndText.Add("3.3.2", "From our observations of the first week of the Summer Institute,  the community college students, much like the university students  in the Educational Game Design Class, were able to exhibit  transfer. As one of the projects in the Summer Institute, the  students were given general guidelines on how to create various  ecological simulations such as predator prey models. Using these  general guidelines along with applying the programming skills  and computational thinking patterns they had obtained throughout  the first half of the week, the students were able to create real  world ecological simulations. Furthermore, students were able to  graph and analyze data related to their simulations such as  populations of species.  ");
               namesAndText.Add("3.3.3", "The second week of the Summer Institute focused on not only  creating AgentSheets projects, but also, learning how to teach  game design, computational science, and programming concepts  with AgentSheets projects. Most of the middle school teachers  had very little, if any, prior programming experience and many  had never taught a programming unit in their middle school  classes. During the second week, through video game design,  teachers and community college students were able to learn key  educational concepts that allowed them to teach programming  units in their classes. Furthermore, the inherent appeal of video  games enables teachers to have a captive student audience for  their programming unit (see middle school experience above). At  present time, these teachers have successfully completed 50  different classes, and as mentioned above, over 1,100 video games  from individual middle school students have been created.  Moreover, these video game units have not been restricted to  computer education classes; for example, in a local school district,  teachers are trying to integrate the creation of AgentSheets video  games as part of the middle school Spanish curriculum.  ");
               viewRangesOnDevice.Add("3.2.1", CreateViewRange(new Rectangle(4, 35, 28, 20), "head"));
               viewRangesOnDevice.Add("3.2.2", CreateViewRange(new Rectangle(35, 35, 8, 20), "rect"));
               viewRangesOnDevice.Add("3.2.3", CreateViewRange(new Rectangle(46, 35, 11, 20), "rect"));
               viewRangesOnDevice.Add("3.2.4", CreateViewRange(new Rectangle(60, 35, 9, 20), "rect"));
               viewRangesOnDevice.Add("3.2.5", CreateViewRange(new Rectangle(72, 35, 22, 20), "rect"));
               viewRangesOnDevice.Add("3.2.6", CreateViewRange(new Rectangle(97, 35, 5, 20), "rect"));
               viewRangesOnDevice.Add("3.2.7", CreateViewRange(new Rectangle(4, 5, 3, 20), "rect"));
               viewRangesOnDevice.Add("3.3.1", CreateViewRange(new Rectangle(10, 5, 38, 20), "head"));
               viewRangesOnDevice.Add("3.3.2", CreateViewRange(new Rectangle(51, 5, 17, 20), "rect"));
               viewRangesOnDevice.Add("3.3.3", CreateViewRange(new Rectangle(71, 5, 28, 20), "rect"));
           }

           // page 5 - Game Design
           if (nameOfPaper == "paper3" && pageNum == 5)
           {
               namesAndText.Add("3.3.4", "A surprising emerging development we found through  interviewing community college students after the Institute was  that many of these students decided to continue their computer  science education beyond the community college as a direct result  of their exposure to this project. Multiple students stated that they  became motivated to pursue further computer science education,  some even computer education paths, while helping middle school  teachers create AgentSheets video games. Immediately following  the Summer Institute, one community college student transferred  to a 4-year university and three more students are preparing their  transfer for this coming academic year.   ");
               namesAndText.Add("4.1", "4. DISCUSSION  Video games have universal appeal to audiences ranging from  young children to adults. Scalable Game Design in the middle  school computer education classes increases student engagement  over a variety of demographics and enables the development of  more effective teaching strategies. At the university level, results  show that video game creation could be used as a springboard into  computational thinking, education, and as an avenue to better  represent and explain one’s own discipline to others. Similar to  students, middle school teachers with little to no prior  programming experience can effectively teach game design to  their classes. Finally, it was observed that Scalable Game Design  has the ability to get middle school students as well as college  level students interested in the field of computational science.    ");
               namesAndText.Add("5.1", "5. ACKNOWLEDGMENTS  This material is based in part upon work supported by the  National Science Foundation under Grant Numbers No. 0833612  and DMI-0712571 and DGE-0841423. Any opinions, findings,  and conclusions or recommendations expressed in this material  are those of the authors and do not necessarily reflect the views of  the National Science Foundation. Special thanks to Andri  Ioannidou and Fred Gluck for their valuable input.   ");
               namesAndText.Add("6", "6. REFERENCES  ");
               namesAndText.Add("6.1", "[1] Cooper, S., Dann, W., Pausch, R., Teaching Objects-first In  Introductory Computer Science, In Proc. SIGCSE 2003,  Reno, Nevada, USA, 2003  ");
               namesAndText.Add("6.2", "[2] Peppler, K. & Kafai, Y. B., Collaboration, Computation, and  Creativity: Media Arts Practices in Urban Youth Culture. In  C. Hmelo- Silver & A. O'Donnell (Eds.), In Proc. Computer  Supported Collaborative Learning, New Brunswick, NJ,  USA, 2007  ");
               namesAndText.Add("6.3", "[3] Repenning, A., Excuse me, I need better AI! Employing  Collaborative Diffusion to make Game AI Child's Play. In  Proc. ACM SIGGRAPH Video Game Symposium, Boston,  MA, USA, ACM Press, 2006.  ");
               namesAndText.Add("6.4", "[4] Sturtevant, N. R., Hoover, H. J., Schaeffer, J., Gouglas, S.,  Bowling, M. H., Southey, F., Bouchard, M., and Zabaneh, G.  2008. Multidisciplinary students and instructors: a second- year games course. In proc 39th SIGCSE Technical  Symposium on Computer Science Education, Portland, OR,  USA, 2008.  ");
               namesAndText.Add("6.5", "[5] Squire, K., Video games in education. International Journal  of Intelligent Simulations and Gaming, (2) 1. 2003  ");
               namesAndText.Add("6.6", "[6] Lewis, C., and Repenning, A., 'Creating Educational  Gamelets,' in Educating Learning Technology Designers:  Guiding and Inspiring Creators of Innovative Educational  Tools, C. DiGiano, S. Goldman, and M. Chorost, Eds. New  York: Routledge, 203-229, 2008  ");
               namesAndText.Add("6.7", "[7] Repenning, A., Basawapatna, A., and Koh, K. H., Making  university education more like middle school computer club:  facilitating the flow of inspiration. In Proc. 14th WCCCE  2009, Burnaby, British Columbia, Canada, 2009  ");
               namesAndText.Add("6.8", "[8] Repenning, A.,  'AgentSheets®: an Interactive Simulation  Environment with End-User Programmable Agents,'In Proc.  Interaction 2000, Tokyo, Japan, 2000   ");
               namesAndText.Add("6.9", "[9] Repenning, A., Webb, D., and Ioannidou, A., 'Scalable  Game Design and the Development of a Checklist for  Getting Computational Thinking into Public Schools,' In  Proc. SIGCSE 2010, Milwaukee, WI, 2010.  ");
               namesAndText.Add("6.10", "[10] Wing, J. M., 'Computational Thinking,' Communications of  the ACM, 49(3), pp. 33-35, March 2006. ");
               namesAndText.Add("6.11", "[11] Salen, K.  Zimmerman, E., Rules of Play: Game Design  Fundamentals, MIT Press, 334-337, 2004  ");
               viewRangesOnDevice.Add("3.3.4", CreateViewRange(new Rectangle(4, 35, 16, 20), "rect"));
               viewRangesOnDevice.Add("4.1", CreateViewRange(new Rectangle(23, 35, 24, 20), "head"));
               viewRangesOnDevice.Add("5.1", CreateViewRange(new Rectangle(50, 35, 13, 20), "head"));
               viewRangesOnDevice.Add("6", CreateViewRange(new Rectangle(66, 35, 4, 20), "head"));
               viewRangesOnDevice.Add("6.1", CreateViewRange(new Rectangle(73, 35, 3, 20), "rect"));
               viewRangesOnDevice.Add("6.2", CreateViewRange(new Rectangle(4, 5, 6, 20), "rect"));
               viewRangesOnDevice.Add("6.3", CreateViewRange(new Rectangle(13, 5, 5, 20), "rect"));
               viewRangesOnDevice.Add("6.4", CreateViewRange(new Rectangle(21, 5, 6, 20), "rect"));
               viewRangesOnDevice.Add("6.5", CreateViewRange(new Rectangle(30, 5, 3, 20), "rect"));
               viewRangesOnDevice.Add("6.6", CreateViewRange(new Rectangle(36, 5, 6, 20), "rect"));
               viewRangesOnDevice.Add("6.7", CreateViewRange(new Rectangle(45, 5, 5, 20), "rect"));
               viewRangesOnDevice.Add("6.8", CreateViewRange(new Rectangle(53, 5, 3, 20), "rect"));
               viewRangesOnDevice.Add("6.9", CreateViewRange(new Rectangle(59, 5, 5, 20), "rect"));
               viewRangesOnDevice.Add("6.10", CreateViewRange(new Rectangle(67, 5, 3, 20), "rect"));
               viewRangesOnDevice.Add("6.11", CreateViewRange(new Rectangle(73, 5, 3, 20), "rect"));
           }

            #endregion

        }

        #region Getters

        // return the dictionary of titles and their text
        public Dictionary<String, String> GetNamesAndText()
        {
            return namesAndText;
        }

        // return the dictonary of view ranges and their titles
        public OrderedDictionary GetViewRanges()
        {
            return viewRangesOnDevice;
        }

        #endregion

        // create a view range of the given size with the give type
        private BrailleIOViewRange CreateViewRange(Rectangle viewRect, String type)
        {
            BrailleIOViewRange viewRange = new BrailleIOViewRange(viewRect.X, viewRect.Y, viewRect.Width, viewRect.Height);  // create a view range with the same dimensions and location as the given rectangle
            Bitmap b = new Bitmap(viewRect.Width, viewRect.Height);                      // initialize a bitmap to use as the image for the view range
            using (Graphics g = Graphics.FromImage(b)) FillGraphics(g, viewRect, type);  // fill the bitmap according to the specified type
            viewRange.SetBitmap(b);                                                      // set the view range contents to be the bitmap
            return viewRange;
        }

        // given a graphics object, rectangle, and type, fill the graphics object according to the type
        private void FillGraphics(Graphics g, Rectangle viewRect, String type)
        {
            if (type == "rect") g.FillRectangle(new SolidBrush(Color.Black), 0, 0, viewRect.Width, viewRect.Height);  // for a rectangle - fill in the given rectangle
            if (type == "head")  // for a heading 
            {
                g.FillRectangle(new SolidBrush(Color.Black), 0, 0, 2, viewRect.Height);                 // add the first two lines of the heading
                g.FillRectangle(new SolidBrush(Color.Black), 3, 0, viewRect.Width, viewRect.Height);    // skip a line, fill in the rest of the rectangle
            }
            if (type == "img")   // for an image
            {
                g.DrawRectangle(new Pen(Color.Black), 0, 0, viewRect.Width - 1, viewRect.Height - 1);                     // draw the outline of the rectangle
                g.DrawRectangle(new Pen(Color.Black), new Rectangle(4, 4, viewRect.Width - 9, viewRect.Height - 9));      // draw a smaller outline of the rectangle 3 dots in from the larger outline
            }
            if (type == "tab")   // for a table
            {
                g.DrawRectangle(new Pen(Color.Black), 0, 0, viewRect.Width - 1, viewRect.Height - 1);                                                   // draw the outline of the rectangle
                for (int i = viewRect.Height - 5; i > 0; i -= 4) g.DrawLine(new Pen(Color.Black), new Point(0, i), new Point(viewRect.Width - 1, i));   // draw a vertical line every 4th space, from left to right
            }
        }
    }
}
